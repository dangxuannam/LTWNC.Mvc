using System;
using Hangfire;
using Hangfire.SqlServer;
using CheapDeal.WebApp.Services;

namespace CheapDeal.WebApp
{
    public static class HangfireConfig
    {
        public static void Register(string connectionString)
        {
            GlobalConfiguration.Configuration
                .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                });

            RegisterRecurringJobs();
        }

        public static void RegisterRecurringJobs()
        {
            var jobManager = new RecurringJobManager();
            var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            // Tuần 11: 8:00 sáng — quét kỳ đến hạn
            jobManager.AddOrUpdate(
                recurringJobId: "daily-contract-jobs",
                methodCall: () => ContractJobService.RunDailyJobs(7),
                cronExpression: "0 8 * * *",
                options: new RecurringJobOptions { TimeZone = tz }
            );

            // Tuần 11: Mỗi giờ — mark overdue
            jobManager.AddOrUpdate(
                recurringJobId: "hourly-mark-overdue",
                methodCall: () => ContractJobService.MarkOverdueSchedules(),
                cronExpression: "0 * * * *",
                options: new RecurringJobOptions { TimeZone = tz }
            );

            // Tuần 12: 9:00 sáng — gửi email reminder
            jobManager.AddOrUpdate(
                recurringJobId: "send-pending-emails",
                methodCall: () => ContractJobService.SendPendingEmails(),
                cronExpression: "0 9 * * *",
                options: new RecurringJobOptions { TimeZone = tz }
            );
        }
    }
}