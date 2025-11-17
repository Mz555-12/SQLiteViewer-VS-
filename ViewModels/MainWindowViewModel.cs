using GalaSoft.MvvmLight;
using SQLiteViewer.Base;
using SQLiteViewer.Models;
using SQLiteViewer.Mothods;
using System;
using System.Collections.Generic;
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

        private FileExplorerViewModel _fileExplorerViewModel;
        public FileExplorerViewModel FileExplorerViewModel
        {
            get
            {
                if (_fileExplorerViewModel == null)
                {
                    _fileExplorerViewModel = new FileExplorerViewModel();
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
                            MessageBox.Show($"选中的文件: {path}");

                            // 这里可以调用你的数据库打开方法
                            // connetHtml.OpenHtmlWithData(path);
                        }
                    });
                }
                return _fileSelectedCommand;
            }
        }

    }
}
