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
    public class FileSystemService
    {
        public ObservableCollection<FileSystemItem> LoadFileSystem(string rootPath)
        {
            var items = new ObservableCollection<FileSystemItem>();

            if (!Directory.Exists(rootPath))
                return items;

            try
            {
                
                // 添加所有文件夹（不进行过滤）
                var directories = Directory.GetDirectories(rootPath);
                foreach (var dir in directories)
                {
                    var dirInfo = new DirectoryInfo(dir);
                    var item = new FileSystemItem
                    {
                        Name = dirInfo.Name,
                        FullPath = dirInfo.FullName,
                        IsDirectory = true
                    };
                    items.Add(item);
                }

                // 添加文件
                var files = Directory.GetFiles(rootPath, "*.db");
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    var item = new FileSystemItem
                    {
                        Name = fileInfo.Name,
                        FullPath = fileInfo.FullName,
                        IsDirectory = false
                    };
                    items.Add(item);
                }
            }
            catch
            {
                // 处理无权限访问等情况
            }

            return items;
        }

        // 检查文件夹或其子文件夹是否包含 .db 文件
        private bool ContainsDbFiles(string directoryPath)
        {
            try
            {
                // 检查当前文件夹是否包含 .db 文件
                if (Directory.GetFiles(directoryPath, "*.db").Any())
                    return true;

                // 递归检查子文件夹
                foreach (var subDir in Directory.GetDirectories(directoryPath))
                {
                    if (ContainsDbFiles(subDir))
                        return true;
                }
            }
            catch
            {
                // 处理无权限访问等情况
            }

            return false;
        }

        public void LoadChildren(FileSystemItem parentItem)
        {
            if (!parentItem.IsDirectory || parentItem.Children.Any())
                return;

            try
            {
                var children = LoadFileSystem(parentItem.FullPath);
                foreach (var child in children)
                {
                    parentItem.Children.Add(child);
                }
            }
            catch
            {
                // 处理访问错误
            }
        }
    }
}
