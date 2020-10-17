namespace WorkReportCreator.Models
{
    public class DynamicTask
    {
        /// <summary>
        /// Описание работы
        /// </summary>
        public string Description { get; set; } = "";

        public DynamicTask()
        {
        }

        public DynamicTask(string description) => Description = description;
    }
}
