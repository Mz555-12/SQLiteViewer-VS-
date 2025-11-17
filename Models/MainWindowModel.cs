using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SQLiteViewer.Models
{
    public class MainWindowModel
    {
        private static MainWindowModel _instance;

        public static MainWindowModel Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MainWindowModel();
                return _instance;
            }
        }

        public WebBrowser webBrowser { get; set; }
    }
}
