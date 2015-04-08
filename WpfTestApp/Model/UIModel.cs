using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Threading;
using PropertyChanged;
using WpfTestApp.Tasks;

namespace WpfTestApp.Model
{
    public interface IUIModel
    {
        bool Busy { get; }
        void CancelOperations();
        CancellationTokenSource CancellationSource { get; }
    }

    [ImplementPropertyChanged]
    public class UIModel : IUIModel
    {
        [AlsoNotifyFor("SearchEnabled")]
        public bool Busy { get; set; }

        public ConcurrentBag<string> Logs { get; set; }

        [AlsoNotifyFor("SearchEnabled")]
        public string Keyword { get; set; }
        [AlsoNotifyFor("SearchEnabled")]
        public string DirectoryToSearch { get; set; }

        public ObservableCollection<Job> CompletedJobs { get; set; }
        public JobRunner JobRunner { get; set; }
        public CancellationTokenSource CancellationSource { get; private set; }

        public int JobCount { get; set; }
        public bool HasJobs { get { return JobCount > 0; } }

        public bool SearchEnabled
        {
            get { return !Busy && !String.IsNullOrEmpty(Keyword) && !String.IsNullOrEmpty(DirectoryToSearch); }
        }

        public UIModel()
        {
            Logs = new ConcurrentBag<string>();
            DirectoryToSearch = Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments,
                Environment.SpecialFolderOption.DoNotVerify);
            CompletedJobs = new ObservableCollection<Job>();
            JobRunner = new JobRunner(CompletedJobs, OnProgressCallback, TimeSpan.FromMilliseconds(500));

            CancellationSource = new CancellationTokenSource();
            JobRunner.CancellationSource = CancellationSource;
        }

        private void OnProgressCallback(string progress)
        {
            AddLog(progress);
            JobCount = JobRunner.JobCount;
        }

        [AlsoNotifyFor("Logs")]
        private int LogsChanged { get; set; }

        public void AddLog(string log)
        {
            Logs.Add(String.Format("[{0}] {1}", DateTime.Now, log));
            LogsChanged++;
        }

        public void AddLog(string formatString, params object[] args)
        {
            Logs.Add(String.Format("[{0}]", DateTime.Now));
            Logs.Add(String.Format(formatString, args));
            LogsChanged++;
        }

        public void CancelOperations()
        {
            CancellationSource.Cancel();
            JobRunner.Stop();
            CancellationSource = new CancellationTokenSource();
        }

        public void BeginOperations()
        {
            CancellationSource = new CancellationTokenSource();
            JobRunner.CancellationSource = CancellationSource;
            JobRunner.Start();
        }
    }
}
