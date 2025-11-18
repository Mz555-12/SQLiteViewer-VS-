using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteViewer.Models
{
    public class SettingModel
    {
        private static SettingModel _instance;
        public static SettingModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SettingModel();
                }
                return _instance;
            }
        }

        public string setting_selectedFolderPath;
        // 历史记录列表
        public ObservableCollection<FileHistoryItem> FileHistory { get; set; } = new ObservableCollection<FileHistoryItem>();
    }
}
