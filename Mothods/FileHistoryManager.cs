using SQLiteViewer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteViewer.Mothods
{
    public class FileHistoryManager
    {
        private static FileHistoryManager _instance;

        
        public static FileHistoryManager Instance
        {
            get
            {
                
                if (_instance == null)
                {
                    _instance = new FileHistoryManager();
                }

               
                return _instance;
            }
        }



        public ObservableCollection<FileHistoryItem> HistoryItems { get; set; }
        private const int MaxHistoryCount = 30;

        private FileHistoryManager()
        {
            HistoryItems = new ObservableCollection<FileHistoryItem>();
        }

        public void AddFileHistory(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.Name);
                double fileSizeInKB = Math.Round(fileInfo.Length / 1024.0, 2);

                var newItem = new FileHistoryItem
                {
                    FileName = fileNameWithoutExtension,
                    FileSize = fileSizeInKB,
                    TriggerTime = DateTime.Now,
                    OriginPath = filePath
                };

                // 检查是否已存在相同路径的记录
                var existingItem = HistoryItems.FirstOrDefault(item => item.OriginPath == filePath);
                if (existingItem != null)
                {
                    // 移除已存在的记录
                    HistoryItems.Remove(existingItem);
                }

                // 添加到第一位
                HistoryItems.Insert(0, newItem);

                // 限制历史记录数量
                if (HistoryItems.Count > MaxHistoryCount)
                {
                    HistoryItems.RemoveAt(HistoryItems.Count - 1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"添加历史记录时出错: {ex.Message}");
            }
        }

        public void ClearHistory()
        {
            HistoryItems.Clear();
        }

        public void RemoveHistoryItem(FileHistoryItem item)
        {
            HistoryItems.Remove(item);
        }
    }
}
