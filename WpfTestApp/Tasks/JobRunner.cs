using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using PropertyChanged;
using WpfTestApp.Model;

namespace WpfTestApp.Tasks
{
    [ImplementPropertyChanged]
    public class JobRunner
    {
        private Thread[] _workers;
        private readonly IProgressReceiver _progressReceiver;
        private readonly BlockingCollection<Job> _jobQueue;
        private readonly IList<Job> _completedJobsHolder;
        private readonly TimeSpan _queuePollingPeriod;

        public CancellationTokenSource CancellationSource { get; set; }
        public int JobCount { get; private set; }

        public JobRunner(IList<Job> completedJobsHolder, Action<string> progressCallback, TimeSpan queuePollingPeriod)
        {
            _progressReceiver = new ProgressReceiver(progressCallback);
            _jobQueue = new BlockingCollection<Job>();
            _completedJobsHolder = completedJobsHolder;
            _queuePollingPeriod = queuePollingPeriod;
        }

        private void ProcessJob<TWorker>() where TWorker : IWorker, new()
        {
            var workerName = typeof (TWorker).Name;
            var threadId = Thread.CurrentThread.ManagedThreadId;
            _progressReceiver.SendProgress("Starting {0} with thread id {1}.", workerName, threadId);

            while (!CancellationSource.IsCancellationRequested)
            {
                try
                {
                    CancellationSource.Token.ThrowIfCancellationRequested();
                    Job job;
                    if (_jobQueue.TryTake(out job, (int)_queuePollingPeriod.TotalMilliseconds, CancellationSource.Token))
                    {
                        var worker = new TWorker() as IWorker;
                        worker.Execute(job, _progressReceiver, CancellationSource.Token);
                        _completedJobsHolder.Add(job);
                        JobCount = _jobQueue.Count;
                    }
                }
                catch (OperationCanceledException)
                {
                    _progressReceiver.SendProgress("{0} with thread id {1} detected cancellation, exiting", workerName, threadId);
                    break;
                }
            }
        }
        
        public void Start()
        {
            if (_workers == null)
            {
                _workers = new[]
                {
                    new Thread(ProcessJob<DebugDumpWorker>),
                    new Thread(ProcessJob<FileSearchWorker>)
                };

                foreach (Thread t in _workers)
                {
                    t.Start();
                }
            }
        }

        public void Stop()
        {
            if (_workers != null)
            {
                foreach (Thread t in _workers)
                {
                    t.Join(TimeSpan.FromMilliseconds(100));
                }
                _workers = null;
            }
        }

        public void AddJob(Job job)
        {
            _jobQueue.Add(job, CancellationSource.Token);
            JobCount = _jobQueue.Count;
        }
        
        internal class ProgressReceiver : IProgressReceiver
        {
            private readonly Action<string> _progressCallback;
            
            public ProgressReceiver(Action<string> progressCallback)
            {
                _progressCallback = progressCallback;
            }

            public void SendProgress(string progress, params object[] args)
            {
                if (_progressCallback != null)
                {
                    try
                    {
                        if (args == null || args.Length == 0)
                        {
                            _progressCallback(progress);
                        }
                        else
                        {
                            _progressCallback(String.Format(progress, args));
                        }
                    }
                    catch
                    {
                        // ensure someone else mess does not affect us
                        // TODO: Logging these cases
                    }
                }
            }
        }
    }
}
