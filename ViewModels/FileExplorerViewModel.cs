using GalaSoft.MvvmLight;
using SQLiteViewer.Base;
using SQLiteViewer.Models;
using SQLiteViewer.Mothods;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SQLiteViewer.ViewModels
{
    public class FileExplorerViewModel : ObservableObject
    {
        private FileSystemService _fileSystemService;
        private string _selectedPath;

        public ObservableCollection<FileSystemItem> FileSystemItems { get; set; }

        // 添加文件选择事件
        public event EventHandler<string> FileSelected;

        public FileExplorerViewModel()
        {
            _fileSystemService = new FileSystemService();
            FileSystemItems = new ObservableCollection<FileSystemItem>();

            // 默认加载桌面路径，你可以修改为其他路径
            LoadFileSystem("E:\\ColleageLife");
        }

        private void LoadFileSystem(string path)
        {
            FileSystemItems.Clear();
            var items = _fileSystemService.LoadFileSystem(path);
            foreach (var item in items)
            {
                FileSystemItems.Add(item);
            }
        }

        public void OnItemExpanded(FileSystemItem item)
        {
            if (item.IsDirectory)
            {
                _fileSystemService.LoadChildren(item);
            }
        }

        public void OnFileDoubleClicked(FileSystemItem item)
        {
            if (!item.IsDirectory)
            {
                SelectedPath = item.FullPath;
                // 触发文件选择事件
                FileSelected?.Invoke(this, item.FullPath);
            }
        }

        public string SelectedPath
        {
            get { return _selectedPath; }
            set
            {
                Set(ref _selectedPath, value);
            }
        }
    }
}
