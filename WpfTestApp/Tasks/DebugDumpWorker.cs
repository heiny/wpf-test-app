using System;
using System.Collections.Generic;
using System.Threading;
using WpfTestApp.Model;

namespace WpfTestApp.Tasks
{
    internal class DebugDumpWorker : WorkerBase
    {
        protected override void PerformJob(Job job, IProgressReceiver progressReceiver, CancellationToken cancellationToken)
        {
            var items = new List<string>();
            foreach (var prop in job.GetType().GetProperties())
            {
                cancellationToken.ThrowIfCancellationRequested();
                // note: only handles simple types
                if (prop.CanRead)
                {
                    items.Add(GetKeyValue(prop.Name, prop.GetValue(job)));
                }
            }
            
            if (progressReceiver != null)
            {
                progressReceiver.SendProgress(String.Concat("{ ",String.Join(",",items)," }"));
            }
            job.KeywordStatus = KeywordStatus.NotFound;
            Thread.Sleep(100); // HACK: this job is unfair to file searcher which seems to never run
        }

        private string GetKeyValue(string key, object value)
        {
            return String.Format("\"{0}\":\"{1}\"", key, value);
        }
    }
}
