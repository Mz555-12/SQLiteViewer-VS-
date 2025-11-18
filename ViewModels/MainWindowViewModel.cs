using GalaSoft.MvvmLight;
using SQLiteViewer.Base;
using SQLiteViewer.Json;
using SQLiteViewer.Models;
using SQLiteViewer.Mothods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace SQLiteViewer.ViewModels
{
    public class MainWindowViewModel:ObservableObject
    {
        private ConnetHtml connetHtml = new ConnetHtml();
        private MainWindowModel mainWindowModel = MainWindowModel.Instance;
        public FileHistoryManager historyManager { get; set; } = FileHistoryManager.Instance;

        private FileExplorerViewModel _fileExplorerViewModel;

       
        public FileExplorerViewModel FileExplorerViewModel
        {
            get
            {
                if (_fileExplorerViewModel == null)
                {
                    _fileExplorerViewModel = new FileExplorerViewModel(mainWindowModel.selectedFolderPath);
                    // 订阅文件选择事件
                    _fileExplorerViewModel.FileSelected += (sender, filePath) =>
                    {
                        // 当文件被双击时，执行文件选择命令
                        FileSelectedCommand.Execute(filePath);
                    };
                }
                return _fileExplorerViewModel;
            }
        }

        public MainWindowViewModel()
        {
            Setting.Instance.LoadedBaseModel_Json();

            ShowFileName(mainWindowModel.selectedFolderPath);
            FileExplorerViewModel.LoadFileSystem(mainWindowModel.selectedFolderPath);
        }


        private CommandBase _closeMainWindowCommand;

        public CommandBase CloseMainWindowCommand
        {
            get
            {
                if (_closeMainWindowCommand == null)
                {
                    _closeMainWindowCommand = new CommandBase();
                    _closeMainWindowCommand.DoExecute = new Action<object>((o) =>
                    {
                        Setting.Instance.SetBaseModel_Json();

                        (o as Window).Close();
                    });
                }
                return _closeMainWindowCommand;
            }
        }

        /// <summary>
        /// 处理文件选择事件
        /// </summary>
        private CommandBase _fileSelectedCommand;
        public CommandBase FileSelectedCommand
        {
            get
            {
                if (_fileSelectedCommand == null)
                {
                    _fileSelectedCommand = new CommandBase();
                    _fileSelectedCommand.DoExecute = new Action<object>((filePath) =>
                    {
                        string path = filePath as string;
                        
                        if (!string.IsNullOrEmpty(path))
                        {
                            // 添加到历史记录
                            FileHistoryManager.Instance.AddFileHistory(path);

                            connetHtml.OpenHtmlWithData(path);


                            // 保存配置（包含历史记录）
                            Setting.Instance.SetBaseModel_Json();
                        }
                    });
                }
                return _fileSelectedCommand;
            }
        }

        private CommandBase _openFileCommand;
        public CommandBase OpenFileCommand
        {
            get
            {
                if (_openFileCommand == null)
                {
                    _openFileCommand = new CommandBase();
                    _openFileCommand.DoExecute = new Action<object>((obj) =>
                    {
                        string result = obj as string;
                        if (result == "file")
                        {
                            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
                            
                            // 设置文件过滤器，只显示.db文件
                            openFileDialog.Filter = "数据库文件 (*.db)|*.db|所有文件 (*.*)|*.*";
                            openFileDialog.FilterIndex = 1; // 默认选择第一个过滤器

                            if (openFileDialog.ShowDialog() == true)
                            {
                                string selectedFileName = openFileDialog.FileName;

                                // 添加到历史记录
                                FileHistoryManager.Instance.AddFileHistory(selectedFileName);
                                // 处理打开.db文件的逻辑
                                connetHtml.OpenHtmlWithData(selectedFileName);
                            }
                            return;
                        }

                        var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
                        folderDialog.SelectedPath = mainWindowModel.selectedFolderPath;
                        if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            // 处理打开文件夹的逻辑
                            mainWindowModel.selectedFolderPath = folderDialog.SelectedPath;


                            ShowFileName(mainWindowModel.selectedFolderPath);

                            // 展示文件夹内容
                            FileExplorerViewModel.LoadFileSystem(mainWindowModel.selectedFolderPath);

                            Setting.Instance.SetBaseModel_Json();
                        }

                    });

                }
                return _openFileCommand;
            }
        }

        private CommandBase _refurbishFileCommand;
        public CommandBase RefurbishFileCommand
        {
            get
            {
                if (_refurbishFileCommand == null)
                {
                    _refurbishFileCommand = new CommandBase();
                    _refurbishFileCommand.DoExecute = new Action<object>((obj) =>
                    {
                        if(string.IsNullOrEmpty(mainWindowModel.selectedFolderPath))
                        {
                            MessageBox.Show("请先选择文件夹！");
                            return;
                        }
                        ShowFileName(mainWindowModel.selectedFolderPath);

                        
                        FileExplorerViewModel.LoadFileSystem(mainWindowModel.selectedFolderPath);

                    });

                }
                return _refurbishFileCommand;
            }
        }

        public void ShowFileName(string path)
        {
            string folderName = Path.GetFileName(path);
            
            if (string.IsNullOrEmpty(folderName))
            {
                // 获取盘符，比如 "C:\" -> "C盘"
                string driveLetter = Path.GetPathRoot(path).TrimEnd('\\', '/');
                if (string.IsNullOrEmpty(driveLetter))
                {
                    driveLetter = "文件浏览器";
                }
                folderName = driveLetter;
            }
            mainWindowModel.currentFolder_text.Text = folderName;

        }



    }
}
