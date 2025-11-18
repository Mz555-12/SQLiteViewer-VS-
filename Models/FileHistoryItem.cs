using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteViewer.Models
{
    public class FileHistoryItem
    {
        public string FileName { get; set; }
        public double FileSize { get; set; }
        public DateTime TriggerTime { get; set; }
        public string OriginPath { get; set; }

        public FileHistoryItem() { }

        public FileHistoryItem(string fileName, double fileSize, DateTime triggerTime, string originPath)
        {
            FileName = fileName;
            FileSize = fileSize;
            TriggerTime = triggerTime;
            OriginPath = originPath;
        }
    }
}
