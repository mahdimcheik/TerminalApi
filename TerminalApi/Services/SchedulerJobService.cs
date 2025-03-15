using Hangfire;
using TerminalApi.Contexts;

namespace TerminalApi.Services
{
    public class SchedulerJobService
    {
        private readonly ApiDefaultContext context;
        public SchedulerJobService(ApiDefaultContext context)
        {
            var jobId = BackgroundJob.Enqueue(
    () => Console.WriteLine("Fire-and-forget!"));
            this.context = context;
        }

        public  Task ScheduleSingleJob(Action<object> job, object param)
        {
            var jobId = BackgroundJob.Enqueue(() => job.Invoke(param));
            return Task.CompletedTask;
        }

    }
}
