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
        private CommandBase _testCommand;

        public CommandBase TestCommand
        {
            get
            {
                if (_testCommand == null)
                {
                    _testCommand = new CommandBase();
                    _testCommand.DoExecute = new Action<object>((o) =>
                    {
                        //SendDataToHtmlSafely(@"E:\ColleageLife\Learn\C#\\Project\SQLiteViewer\bin\Debug\Html");
                        connetHtml.OpenHtmlWithData(@"E:\ColleageLife\Learn\Mydatabase\mydemo.db");
                        MessageBox.Show("运行完毕");
                    });
                }
                return _testCommand;
            }
        }

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
                        // 这里处理文件选择事件
                        string path = filePath as string;
                        if (!string.IsNullOrEmpty(path))
                        {
                            // 调用你的方法，例如：
                            // connetHtml.OpenHtmlWithData(path);
                            connetHtml.OpenHtmlWithData(path);
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
                                string selectedFilePath = openFileDialog.FileName;
                                // 处理打开.db文件的逻辑
                                connetHtml.OpenHtmlWithData(selectedFilePath);
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

                            // 这里添加您的文件夹处理代码
                            FileExplorerViewModel.LoadFileSystem(mainWindowModel.selectedFolderPath);

                            Setting.Instance.SetBaseModel_Json();
                        }

                    });

                }
                return _openFileCommand;
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
