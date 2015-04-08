using System;
using WpfTestApp.Tasks;

namespace WpfTestApp.Model
{
    public class Job
    {
        public KeywordStatus KeywordStatus { get; set; }
        public string Keyword { get; set; }
        public string FileSearched { get; set; }
        public TimeSpan TimeTaken { get; set; }
        public DateTime DateStarted { get; set; }

        public Job(string fileToSearch, string keywordToSearchFor)
        {
            FileSearched = fileToSearch;
            Keyword = keywordToSearchFor;
            TimeTaken = TimeSpan.Zero;
        }
    }
}
