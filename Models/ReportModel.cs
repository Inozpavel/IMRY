using System.Collections.Generic;

namespace WorkReportCreator.Models
{
    public class ReportModel
    {
        public int WorkNumber { get; set; }

        public string WorkType { get; set; }

        public List<int> SelectedTasksIndices { get; set; }

        public Dictionary<string, string> FilesAndDescriptions { get; set; }
    }
}
