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

    }
}
