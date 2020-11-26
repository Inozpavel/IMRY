using Newtonsoft.Json;
using System.Collections.Generic;

namespace WorkReportCreator.Models
{
    public class ReportModel
    {
        public int WorkNumber { get; set; }

        public string WorkType { get; set; }

        public List<int> SelectedTasksIndices { get; set; } = new List<int>();

        public Dictionary<string, string> FilesAndDescriptions { get; set; } = new Dictionary<string, string>();

        [JsonIgnore]
        public string FilePath { get; set; }

        [JsonIgnore]
        public string ReportName { get; set; }
    }
}
