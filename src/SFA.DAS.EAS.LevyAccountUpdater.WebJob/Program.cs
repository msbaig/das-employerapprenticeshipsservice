using System;
using SFA.DAS.EAS.Infrastructure.Logging;
using SFA.DAS.EAS.LevyAccountUpdater.WebJob.DependencyResolution;
using SFA.DAS.EAS.LevyAccountUpdater.WebJob.Updater;

namespace SFA.DAS.EAS.LevyAccountUpdater.WebJob
{
    class Program
    {
        static void Main(string[] args)
        {
            LoggingConfig.ConfigureLogging();

            var container = IoC.Initialize();

            var updater = container.GetInstance<IAccountUpdater>();

            if (!string.IsNullOrEmpty(args[0]) && !string.IsNullOrEmpty(args[1]))
            {
                updater.RunUpdateForSingleAccountAndScheme(Convert.ToInt64(args[0]),args[1]).Wait();
            }
            else
            {
                updater.RunUpdate().Wait();
            }
            
        }
    }
}
