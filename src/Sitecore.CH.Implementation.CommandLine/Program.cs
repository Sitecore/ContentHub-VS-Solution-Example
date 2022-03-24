using ManyConsole;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Sitecore.CH.Implementation.CommandLine
{
    class Program
    {
        public static Application Application;
        static void Main(string[] args)
        {                    
            Application = new Application();
            Application.Startup();
            ConsoleCommandDispatcher.DispatchCommand(Application.ServiceProvider.GetServices<ConsoleCommand>(), args, Console.Out);
        }
    }
}
