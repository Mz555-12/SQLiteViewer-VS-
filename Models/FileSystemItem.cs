using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteViewer.Models
{
    public class FileSystemItem
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public bool IsDirectory { get; set; }
        public bool IsExpanded { get; set; }
        public ObservableCollection<FileSystemItem> Children { get; set; }

        public FileSystemItem()
        {
            Children = new ObservableCollection<FileSystemItem>();
        }
    }
}
