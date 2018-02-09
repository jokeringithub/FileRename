using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileRenamer
{
    /// <summary>
    /// 文件基本信息类，仅提供与文件重命名有关的文件信息
    /// </summary>
    public class FileBasicInfo : BindableObject
    {
        /// <summary>
        /// 使用FileInfo类实例化此类
        /// </summary>
        /// <param name="fileInfo">FileInfo类实例</param>
        public FileBasicInfo(FileInfo fileInfo)
        {
            InitializeFileBasicInfo(fileInfo);
        }

        /// <summary>
        /// 使用文件路径实例化此类
        /// </summary>
        /// <param name="fileFullName">文件路径，无法打开则创建一个占位符</param>
        /// <exception cref="FileNotFoundException"></exception>
        public FileBasicInfo(string fileFullName)
        {
            try
            { InitializeFileBasicInfo(new FileInfo(fileFullName));  }
            catch (Exception)
            { InitializeDummyFileBasicInfo(); }
        }

        /// <summary>
        /// 使用FileInfo执行初始化
        /// </summary>
        /// <param name="fileInfo">文件的FileInfo</param>
        private void InitializeFileBasicInfo(FileInfo fileInfo)
        {
            selected = true;
            fileName = fileInfo.Name;
            fileExtension = fileInfo.Extension;
            filePath = fileInfo.FullName;
            fileDirectory = fileInfo.DirectoryName;
            fileRenameCompleted = false;
            fileSize = fileInfo.Length;
            fileUpdateTime = fileInfo.LastWriteTime;
        }

        /// <summary>
        /// 初始化一个占位实例
        /// </summary>
        private void InitializeDummyFileBasicInfo()
        {
            selected = true;
            fileName = "***";
            fileExtension = string.Empty;
            filePath = string.Empty;
            fileDirectory = string.Empty;
            fileRenameCompleted = false;
            fileSize = 0;
            fileUpdateTime = new DateTime();
        }

        /// <summary>
        /// 判断是否为同一文件
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            { return false; }
            else if (((FileBasicInfo)obj).FilePath != this.filePath)
            { return false; }
            else
            { return true; }
        }

        /// <summary>
        /// 生成基于文件路径的哈希值
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return -1223983128 + EqualityComparer<string>.Default.GetHashCode(fileName);
        }

        private bool selected;
        /// <summary>
        /// 指示是否被选中
        /// </summary>
        public bool Selected { get => selected;  set => SetProperty(ref selected, value); }

        private string fileName;
        /// <summary>
        /// 文件名字符串
        /// </summary>
        public string FileName { get => fileName;  }

        private string fileExtension;
        /// <summary>
        /// 文件扩展名
        /// </summary>
        public string FileExtension { get => fileExtension; }

        private string fileDirectory;
        /// <summary>
        /// 文件所在目录
        /// </summary>
        public string FileDirectory { get => fileDirectory; }

        private string fileNewName;
        /// <summary>
        /// 新文件名字符串
        /// </summary>
        public string FileNewName { get => fileNewName; set => SetProperty(ref fileNewName, value); }

        private string filePath;
        /// <summary>
        /// 文件完整绝对路径
        /// </summary>
        public string FilePath { get => filePath; }

        /// <summary>
        /// 文件重命名后的新路径
        /// </summary>
        public string FileNewPath { get => fileDirectory + @"\" + fileNewName; }

        private bool fileRenameCompleted;
        /// <summary>
        /// 指示文件重命名是否完成
        /// </summary>
        public bool FileRenameCompleted { get => fileRenameCompleted; set => SetProperty(ref fileRenameCompleted, value); }

        private long fileSize;
        /// <summary>
        /// 文件大小，单位Byte
        /// </summary>
        public long FileSize { get => fileSize; }
        /// <summary>
        /// 文件大小的字符串，会自动转换为KB, MB, GB为单位
        /// </summary>
        public string FileSizeString
        {
            get
            {
                if (fileSize < Math.Pow(1024, 2))
                {
                    return Math.Round((fileSize / Math.Pow(1024, 1)), 2).ToString() + " KB";
                }
                else if (fileSize < Math.Pow(1024, 3))
                {
                    return Math.Round((fileSize / Math.Pow(1024, 2)), 2).ToString() + " MB";
                }
                else
                {
                    return Math.Round((fileSize / Math.Pow(1024, 3)), 2).ToString() + " GB";
                }
            }
        }

        private DateTime fileUpdateTime;
        /// <summary>
        /// 文件修改日期
        /// </summary>
        public DateTime FileUpdateTime { get => fileUpdateTime; }
        /// <summary>
        /// 文件修改日期字符串，格式为"短日期 短时间"
        /// </summary>
        public string FileUpdateTimeString { get => fileUpdateTime.ToShortDateString() + " " + fileUpdateTime.ToShortTimeString(); }
    }
}
