using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteViewer.Mothods
{
    public class FileInfoItem
    {
        // 字典存储文件信息
        public static Dictionary<string, object> fileInfoDic { get; set; }= new Dictionary<string, object>();
        public static Dictionary<string,object> GetFileInfo(string filepath)
        {
            try
            {
                // 获取文件信息
                FileInfo file = new FileInfo(filepath);

                // 去掉文件名的后缀
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);

                // 将文件大小转换为KB（保留2位小数）
                double fileSizeInKB = Math.Round(file.Length / 1024.0, 2);

                // 存储到字典
                fileInfoDic["FileName"] = fileNameWithoutExtension;
                fileInfoDic["FileSize"] = fileSizeInKB; 
                fileInfoDic["TriggerTime"] = DateTime.Now; // 触发时间
                
                
            }
            catch (Exception ex)
            {
                // 处理文件访问异常
                Console.WriteLine($"获取文件信息时出错: {ex.Message}");
                fileInfoDic["Error"] = ex.Message;
            }
            return fileInfoDic;

        }
    }
}
