using System;
using System.Diagnostics;
using System.Threading;
using WpfTestApp.Model;

namespace WpfTestApp.Tasks
{
    public abstract class WorkerBase : IWorker
    {
        protected abstract void PerformJob(Job job, IProgressReceiver progressReceiver, CancellationToken cancellationToken);

        public void Execute(Job job, IProgressReceiver progressReceiver, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();

            try
            {
                job.DateStarted = DateTime.UtcNow;
                stopwatch.Start();
                PerformJob(job, progressReceiver, cancellationToken);
            }
            catch (Exception ex)
            {
                progressReceiver.SendProgress("Error performing job on worker {0}: {1}", Thread.CurrentThread.ManagedThreadId, ex.Message);
            }
            finally
            {
                stopwatch.Stop();
                job.TimeTaken = stopwatch.Elapsed;
            }
        }
    }
}
