﻿using System.Threading.Tasks;

namespace SFA.DAS.EAS.LevyAccountUpdater.WebJob.Updater
{
    public interface IAccountUpdater
    {
        Task RunUpdate();
        Task RunUpdateForSingleAccountAndScheme(long accountId, string payeRef);
    }
}