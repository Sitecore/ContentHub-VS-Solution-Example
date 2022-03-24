using CsvHelper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Sitecore.CH.Base.Features.Base.Extensions;
using Sitecore.CH.Base.Features.Logging.Services;
using Sitecore.CH.Base.Features.SDK.Services;
using Sitecore.CH.Base.Features.SDK.Services.Config;
using Stylelabs.M.Base.Querying;
using Stylelabs.M.Base.Querying.Linq;
using Stylelabs.M.Framework.Essentials.LoadConfigurations;
using Stylelabs.M.Framework.Utilities;
using Stylelabs.M.Sdk.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sitecore.CH.Base.Features.CRUD.Services
{
    public interface IMassDeleteJobService
    {
        Task CreateDeleteJobsAsync(IList<long> entityIds, string definitionName);
        Task CreateDeleteJobsAsync(string csvFilePath, string definitionName);
    }

    class MassDeleteJobService : IMassDeleteJobService
    {
        private readonly ILoggerService<MassDeleteJobService> _logger;
        private readonly IMClientFactory _mClientFactory;
        private readonly MClientOptions _mClientOptions;

        private int _batchCount = 100;
        private int _maxGetManyBatch = 50;

        private int _defaultSleep = 20000;
        private int _defaultNumberOfParalellJobs = 1;
        private int _defaultNumberOfTimesToReportOnWait = 6;

        private const string Job = Stylelabs.M.Sdk.Constants.Job.DefinitionName;
        private const string JobDescription = Stylelabs.M.Sdk.Constants.Job.DefinitionName;
        private const string PendingStatus = "Pending";
        private const string MassEditJobTypeValue = "MassEdit";
        private const string JobTypeProperty = Stylelabs.M.Sdk.Constants.Job.Type;
        private const string JobConditionProperty = Stylelabs.M.Sdk.Constants.Job.Condition;
        private const string JobConfigurationProperty = "Job.Configuration";
        private const string JobStateProperty = "Job.State";
        private const string JobToJobDescriptionRelation = "JobToJobDescription";
        private const string JobStateCompleted = "Completed";
        private const string DoneCSVEntry = "Done";
        private const string JobTargetCount = "Job.TargetCount";
        private const string JobTargetsCompleted = "Job.TargetsCompleted";
        private const string JobPriority = "Job.Priority";


        public static string BatchProcessed(long batchId, long batchCount, string name = "", long? totalItems = null) => $"{name} Batch {batchId} out of {batchCount} has been processed{(totalItems.HasValue ? $". Total items: {totalItems.Value}" : "")}";
        public static string
    IncorrectDefinitionEntities(string definitionName,
        Dictionary<string, IEnumerable<long>> incorrectDefinitionEntities) =>
    $"The chosen definition: {definitionName}. Incorrect entities: {string.Join(",", incorrectDefinitionEntities.Select(x => $"Definition:{x.Key}, Entities: {string.Join(",", x.Value)}"))}";

        public static string MissingEntities(List<long> missingEntities) =>
            $"Missing entities in the system: {string.Join(",", missingEntities)}";

        public static string FileDoesNotExist(string fileName) => $"File {fileName} does not exist";

        public class EntityDelete
        {
            public long EntityId { get; set; }
            public string Status { get; set; }
            public DateTime? DoneDate { get; set; }
        }

        public MassDeleteJobService(ILoggerService<MassDeleteJobService> logger, IMClientFactory mClientFactory, IOptions<MClientOptions> options)
        {
            this._logger = logger;
            this._mClientFactory = mClientFactory;
            this._mClientOptions = options.Value;
        }

        public async Task CreateDeleteJobsAsync(string csvFilePath, string definitionName)
        {
            var listOfEntities = ReadCSV(csvFilePath);

            await CreateDeleteJobsInternal(listOfEntities, definitionName, (results) => StoreItems(csvFilePath, results)).ConfigureAwait(false);
        }

        public async Task CreateDeleteJobsAsync(IList<long> entityIds, string definitionName)
        {
            var listOfEntities = entityIds.Select(entityId => new EntityDelete() { EntityId = entityId });

            await CreateDeleteJobsInternal(listOfEntities, definitionName, (_) => { }).ConfigureAwait(false);
        }


        private async Task CreateDeleteJobsInternal(IEnumerable<EntityDelete> entitiesToDelete, string definitionName, Action<IEnumerable<EntityDelete>> storeAction)
        {
            Guard.NotNullOrEmpty(nameof(definitionName), definitionName);
            var client = _mClientFactory.Client;
            var results = await FilterForRelevant(entitiesToDelete, definitionName).ConfigureAwait(false);

            if (results.Any())
            {
                var batches = results.SplitIntoBatches(_batchCount);

                _logger.LogInformation($"Got {results.Count} to delete in {batches.Count()} batches of {_batchCount}");
                int batchCount = 1;
                var processedItemCount = 0;
                foreach (var item in batches)
                {

                    await WaitAction("Mass Delete jobs", WaitForProcessingToBeDone).ConfigureAwait(false);

                    var jobId = await CreateMassEditJob(item).ConfigureAwait(false);

                    Task<bool> WaitForMassEditJobToCompleteWrapper(int nrwait)
                    {
                        return WaitForMassEditJobToComplete(jobId, nrwait);
                    }
                    await WaitAction($"Mass Edit Job - {jobId}", WaitForMassEditJobToCompleteWrapper).ConfigureAwait(false);

                    storeAction(results);
                    //StoreItems(csvFilePath, results);

                    batchCount++;
                    processedItemCount += item.Count();

                    if (batchCount % 5 == 0)
                    {
                        _logger.LogInformation($"Requested delete {processedItemCount}/{results.Count}");
                    }
                }
            }

            _logger.LogInformation($"Done - Requested {results.Count} entities to be deleted!!!");
        }

        private void StoreItems(string csvFilePath, IEnumerable<EntityDelete> results)
        {
            using (var writer = new StreamWriter(csvFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(results);
            }
        }

        private async Task<bool> WaitForMassEditJobToComplete(long jobId, int nrWaitedTimes)
        {
            var client = _mClientFactory.Client;
            var job = await client.Entities.GetAsync(jobId, EntityLoadConfiguration.Default).ConfigureAwait(false);
            var jobState = job.GetPropertyValue<string>(JobStateProperty);

            var shouldWait = jobState != JobStateCompleted;

            if (shouldWait && nrWaitedTimes % _defaultNumberOfTimesToReportOnWait == 0)
            {
                _logger.LogInformation($"Still waiting for Massedit job with id {job.Id} to complete...");
            }

            return shouldWait;
        }

        private async Task<long> CreateMassEditJob(IEnumerable<EntityDelete> item)
        {
            var client = _mClientFactory.Client;

            var job = await client.EntityFactory.CreateAsync(Job).ConfigureAwait(false);
            long zeroValue = 0;
            job.SetPropertyValue(JobTargetCount, zeroValue);
            job.SetPropertyValue(JobTargetsCompleted, zeroValue);
            job.SetPropertyValue(JobStateProperty, "Created");
            job.SetPropertyValue(JobConditionProperty, PendingStatus);
            job.SetPropertyValue(JobPriority, 0);
            job.SetPropertyValue(JobTypeProperty, MassEditJobTypeValue);

            var jobId = await client.Entities.SaveAsync(job).ConfigureAwait(false);

            await CreateJobDescription(jobId, item).ConfigureAwait(false);

            job = await client.Entities.GetAsync(jobId, EntityLoadConfiguration.Default).ConfigureAwait(false);

            job.SetPropertyValue(JobStateProperty, PendingStatus);

            var dateToSet = DateTime.UtcNow;
            item.ToList().ForEach(i => { i.Status = DoneCSVEntry; i.DoneDate = dateToSet; });

            await client.Entities.SaveAsync(job).ConfigureAwait(false);

            return job.Id.Value;
        }

        private async Task CreateJobDescription(long jobId, IEnumerable<EntityDelete> item)
        {
            var client = _mClientFactory.Client;

            var jobDescription = await client.EntityFactory.CreateAsync(JobDescription).ConfigureAwait(false);

            var jobDescriptionJson = "{'$type':'Stylelabs.M.Base.MassEdit.MassEditJobDescription, Stylelabs.M.Base','Operations':[{'$type':'Stylelabs.M.Base.MassEdit.DeleteEntityOperation, Stylelabs.M.Base'}],'FinalizeOperations':[],'Targets':[]}";

            var token = JToken.Parse(jobDescriptionJson);

            SetToken(token.SelectToken("$..Targets") as JArray, item.Select(i => i.EntityId));

            jobDescription.SetPropertyValue(JobConfigurationProperty, token);

            var jobToJobDescriptionRelation = jobDescription.GetRelation<IChildToOneParentRelation>(JobToJobDescriptionRelation);

            jobToJobDescriptionRelation.Parent = jobId;

            await client.Entities.SaveAsync(jobDescription).ConfigureAwait(false);
        }

        private void SetToken(JValue jValue, bool boolProperty)
        {
            if (jValue != null)
                jValue.Value = boolProperty;
        }

        private void SetToken(JArray jArray, IEnumerable<long> enumerable)
        {
            if (jArray != null)
                jArray.Add(enumerable);
        }

        private void SetToken(JArray jArray, IEnumerable<string> enumerable)
        {
            if (jArray != null)
                jArray.Add(enumerable);
        }

        private async Task<bool> WaitForProcessingToBeDone(int nrWaitedTimes)
        {
            var jobTypeWaitingFor = MassEditJobTypeValue;
            var query = Query.CreateQuery(entities =>
               from e in entities
               where
               e.DefinitionName == Job &&
               e.Property(JobConditionProperty) == PendingStatus &&
               e.Property(JobTypeProperty) == jobTypeWaitingFor &&
               e.CreatedByUsername == _mClientOptions.UserName
               select e);

            var client = _mClientFactory.Client;
            var jobsPending = await client.Querying.QueryIdsAsync(query).ConfigureAwait(false);

            var shouldWait = jobsPending.TotalNumberOfResults >= _defaultNumberOfParalellJobs;

            if (shouldWait && nrWaitedTimes % _defaultNumberOfTimesToReportOnWait == 0)
            {
                _logger.LogInformation($"Still waiting for {jobsPending.TotalNumberOfResults} {jobTypeWaitingFor} jobs");
            }

            return shouldWait;
        }

        private async Task WaitAction(string actionName, Func<int, Task<bool>> shouldWaitFunction)
        {
            _logger.LogInformation($"Going to Wait {actionName}...");
            var keepWaiting = true;
            int timesWaited = 0;
            while (keepWaiting)
            {
                keepWaiting = await shouldWaitFunction(timesWaited).ConfigureAwait(false);

                if (keepWaiting)
                {
                    await Task.Delay(_defaultSleep).ConfigureAwait(false);
                    timesWaited++;
                }
            }

            _logger.LogInformation($"Finished waiting - {actionName}...");
        }

        private async Task<List<EntityDelete>> FilterForRelevant(IEnumerable<EntityDelete> results, string definitionName)
        {
            var client = _mClientFactory.Client;
            _logger.LogInformation($"Start filtering result");
            results = results.Where(i => i.Status != DoneCSVEntry).ToList();
            var filteredResult = results.SplitIntoBatches(_maxGetManyBatch);

            var counter = 0;
            List<long> missingEntities = new List<long>();
            Dictionary<long, string> incorrectDefinitionEntitiesDict = new Dictionary<long, string>();

            foreach (var result in filteredResult)
            {
                counter++;
                var entities = await client.Entities.GetManyAsync(result.Select(x => x.EntityId)).ConfigureAwait(false);
                if (entities.Count() != result.Count())
                {
                    var missingEntitiesFromResult = result.Select(x => x.EntityId).Except(entities.Select(x => x.Id.Value));
                    missingEntities.AddRange(missingEntitiesFromResult);
                }

                if (entities.Any(x => x.DefinitionName != definitionName))
                {
                    var incorrectDefinitionEntities = entities.Where(x => x.DefinitionName != definitionName);
                    foreach (var incorrectDefinitionEntity in incorrectDefinitionEntities)
                        incorrectDefinitionEntitiesDict.Add(incorrectDefinitionEntity.Id.Value, incorrectDefinitionEntity.DefinitionName);
                }
                _logger.LogInformation(BatchProcessed(counter, filteredResult.Count(), "Get Entities ", results.Count()));
            }

            if (incorrectDefinitionEntitiesDict.Any())
            {
                var grouppedDictionary = incorrectDefinitionEntitiesDict.GroupBy(x => x.Value)
                    .ToDictionary(x => x.Key, x => x.Select(y => y.Key));
                _logger.LogError(message: IncorrectDefinitionEntities(definitionName, grouppedDictionary));
                throw new Exception(IncorrectDefinitionEntities(definitionName, grouppedDictionary));
            }
            if (missingEntities.Any())
                _logger.LogWarning(MissingEntities(missingEntities));

            var endresult = results.Where(x => missingEntities.All(y => y != x.EntityId)).ToList();

            _logger.LogInformation($"Found {endresult.Count} entities to be processed.");

            return endresult;
        }

        private List<EntityDelete> ReadCSV(string csvFilePath)
        {
            List<EntityDelete> result;

            if (!File.Exists(csvFilePath))
            {
                var errorMessage = FileDoesNotExist(csvFilePath);
                _logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            using (var reader = new StreamReader(csvFilePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                result = csv.GetRecords<EntityDelete>().ToList();
            }
            return result;
        }

    }
}
