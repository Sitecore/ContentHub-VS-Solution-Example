using System;
using System.Collections.Generic;
using System.Text;

namespace Sitecore.CH.Base.Features.Logging.Services
{
    public interface ILoggingContextService
    {
        object Context { get; }
        void SetContext(object context);

        void ClearContext();
    }

    public class LoggingContextService : ILoggingContextService
    {
        public object Context { get; private set; }
        public void SetContext(object context)
        {
            Context = context;
        }

        public void ClearContext()
        {
            Context = null;
        }
    }
}
