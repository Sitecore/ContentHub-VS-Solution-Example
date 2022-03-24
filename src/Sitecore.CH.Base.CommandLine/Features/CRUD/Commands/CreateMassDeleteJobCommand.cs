using ManyConsole;
using Sitecore.CH.Base.Features.CRUD.Services;

namespace Sitecore.CH.Base.CommandLine.Commands.Features.CRUD.Commands
{
    public class CreateMassDeleteJobCommand : ConsoleCommand
    {
        private const int Success = 0;
        private readonly IMassDeleteJobService _massDeleteJobService;       
        private string _csvFilePath;
        private string _definitionName;
        public CreateMassDeleteJobCommand(IMassDeleteJobService massDeleteJobService)
        {
            IsCommand("create-mass-delete-jobs", "Deletes entities from csv using batch approach");

            HasRequiredOption("filepath=", "path to the text file", path => _csvFilePath = path);
            HasRequiredOption("definitionName=", "entity definition to delete", definitionName => _definitionName = definitionName);
            this._massDeleteJobService = massDeleteJobService;
        }

        public override int Run(string[] remainingArguments)
        {
            _massDeleteJobService.CreateDeleteJobsAsync(_csvFilePath, _definitionName).Wait();
            return Success;
        }
    }
}
