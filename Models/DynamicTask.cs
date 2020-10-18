using System;

namespace WorkReportCreator.Models
{
    public class DynamicTask
    {
        public event Action<object> DescriptionChanged;

        private string _description = "";

        /// <summary>
        /// Описание работы
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                DescriptionChanged?.Invoke(this);
            }
        }

        public DynamicTask()
        {
        }

        /// <param name="description">Описание работы</param>
        public DynamicTask(string description) => Description = description;
    }
}
