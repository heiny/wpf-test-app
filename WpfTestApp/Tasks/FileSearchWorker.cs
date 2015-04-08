using System;
using System.IO;
using System.Threading;
using WpfTestApp.Model;

namespace WpfTestApp.Tasks
{
    internal class FileSearchWorker : WorkerBase
    {
        protected override void PerformJob(Job job, IProgressReceiver progressReceiver, CancellationToken cancellationToken)
        {
            job.KeywordStatus = KeywordStatus.NotFound;

            if (!File.Exists(job.FileSearched))
            {
                progressReceiver.SendProgress("File '{0}' not found!", job.FileSearched);
            }
            else
            {
                try
                {
                    using (var sr = File.OpenText(job.FileSearched))
                    {
                        long lineCount = 1;
                        while (!sr.EndOfStream)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            var line = sr.ReadLine();
                            lineCount++;
                            if (line.Contains(job.Keyword))
                            {
                                job.KeywordStatus = KeywordStatus.Found;
                                progressReceiver.SendProgress("Keyword found on line {0} of file {1}", lineCount, job.FileSearched);
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    progressReceiver.SendProgress("Error searching file: {0}", ex.Message);
                }
            }
        }
    }
}
