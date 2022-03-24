using System;
using System.Collections.Generic;
using System.Text;

namespace Sitecore.CH.Implementation.AzFunctions
{
    public class Constants
    {
        public static class Logging
        {
            public const string TargetIdMissing = "TargetId is missing";
            public const string EntityIsNotValid = "Entity is not valid";
            public const string RequestBodyIsEmpty = "Request body is empty";
            public static string EntityDoesNotExist(long entityId) => $"Entity: {entityId}, does not exist";
            public static string RequestBodyNotSerializable(string requestBody) => $"Request body not serializable: {requestBody}";
            public static string ParentIdMissing(long targetId) => $"ParentId is missing for Target Id {targetId}";
        }
    }
}
