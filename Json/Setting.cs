using Newtonsoft.Json;
using SQLiteViewer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SQLiteViewer.Json
{
    public class Setting
    {
        private static Setting _instance;
        public static Setting Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Setting();
                }
                return _instance;
            }
        }

        private static readonly string ConfigDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WeChatAuto", "Config");

        private static readonly string ConfigFilePath = Path.Combine(
            ConfigDirectory, "SetBase_Config.json");

        public SettingModel settingModel = SettingModel.Instance;
        public MainWindowModel mainWindowModel = MainWindowModel.Instance;

        /// <summary>
        /// 保存配置到JSON文件
        /// </summary>
        /// <param name="config">配置对象</param>
        /// <returns>是否成功保存</returns>
        public bool SaveConfig(SettingModel config)
        {
            try
            {
                // 确保配置目录存在
                if (!Directory.Exists(ConfigDirectory))
                {
                    Directory.CreateDirectory(ConfigDirectory);
                }

                // 序列化配置对象为JSON
                string json = JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);

                // 写入文件
                File.WriteAllText(ConfigFilePath, json);

                return true;
            }
            catch (Exception ex)
            {
                // 在实际应用中，您可以记录日志或抛出异常
                Console.WriteLine($"保存配置失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 从JSON文件加载配置
        /// </summary>
        /// <returns>配置对象，如果文件不存在则返回默认配置</returns>
        public SettingModel LoadConfig()
        {
            try
            {
                // 检查配置文件是否存在
                if (!File.Exists(ConfigFilePath))
                {
                    // 返回默认配置
                    return new SettingModel();
                }

                // 读取文件内容
                string json = File.ReadAllText(ConfigFilePath);

                // 反序列化JSON为配置对象
                return JsonConvert.DeserializeObject<SettingModel>(json) ?? new SettingModel();
            }
            catch (Exception ex)
            {
                // 在实际应用中，您可以记录日志或抛出异常
                Console.WriteLine($"加载配置失败: {ex.Message}");
                return new SettingModel();
            }
        }

        /// <summary>
        /// 检查配置文件是否存在
        /// </summary>
        /// <returns>是否存在配置文件</returns>
        public bool ConfigExists()
        {
            return File.Exists(ConfigFilePath);
        }

        /// <summary>
        /// 删除配置文件
        /// </summary>
        /// <returns>是否成功删除</returns>
        public static bool DeleteConfig()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    File.Delete(ConfigFilePath);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"删除配置失败: {ex.Message}");
                return false;
            }
        }

        public bool SetBaseModel_Json()
        {
            settingModel.setting_selectedFolderPath = mainWindowModel.selectedFolderPath;


            return SaveConfig(settingModel);
        }

        public void LoadedBaseModel_Json()
        {
            SettingModel loadedConfig = LoadConfig();
            mainWindowModel.selectedFolderPath = loadedConfig.setting_selectedFolderPath ;
        }
    }
}
