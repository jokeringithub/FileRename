using System;
using System.ComponentModel;
using System.IO;
using XstarS.ComponentModel;

namespace FileRename
{
    /// <summary>
    /// 文件信息类，仅提供与文件重命名有关的文件信息
    /// </summary>
    public abstract class FileRenameInfo : INotifyPropertyChanged
    {
        /// <summary>
        /// 使用文件路径初始化此类。
        /// </summary>
        /// <param name="filePath">文件路径，无法打开则创建一个占位符。</param>
        /// <exception cref="FileNotFoundException"></exception>
        public FileRenameInfo(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                this.Selected = true;
                this.Name = fileInfo.Name;
                this.Extension = fileInfo.Extension;
                this.FullName = fileInfo.FullName;
                this.Directory = fileInfo.DirectoryName;
                this.RenameCompleted = false;
                this.Size = fileInfo.Length;
                this.UpdateTime = fileInfo.LastWriteTime;
            }
            catch (Exception)
            {
                this.Selected = true;
                this.Name = "***";
                this.Extension = string.Empty;
                this.FullName = string.Empty;
                this.Directory = string.Empty;
                this.RenameCompleted = false;
                this.Size = 0;
                this.UpdateTime = new DateTime();
            }
        }

        /// <summary>
        /// 指示是否被选中。
        /// </summary>
        public abstract bool Selected { get; set; }
        /// <summary>
        /// 文件名。
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// 文件扩展名。
        /// </summary>
        public string Extension { get; }
        /// <summary>
        /// 文件所在目录。
        /// </summary>
        public string Directory { get; }
        /// <summary>
        /// 文件完整路径。
        /// </summary>
        public string FullName { get; }
        /// <summary>
        /// 文件大小，单位 Byte。
        /// </summary>
        public long Size { get; }
        /// <summary>
        /// 文件大小的字符串，会自动转换为 KB, MB, GB 等单位。
        /// </summary>
        public string SizeString =>
            (this.Size < Math.Pow(1024, 2)) ?
            Math.Round(this.Size / Math.Pow(1024, 1), 2).ToString() + " KB" :
            (this.Size < Math.Pow(1024, 3)) ?
            Math.Round(this.Size / Math.Pow(1024, 2), 2).ToString() + " MB" :
            (this.Size < Math.Pow(1024, 4)) ?
            Math.Round(this.Size / Math.Pow(1024, 3), 2).ToString() + " GB" :
            Math.Round(this.Size / Math.Pow(1024, 4), 2).ToString() + " TB";
        /// <summary>
        /// 文件修改日期。
        /// </summary>
        public DateTime UpdateTime { get; }
        /// <summary>
        /// 文件修改日期字符串，格式为 "短日期 短时间"。
        /// </summary>
        public string UpdateTimeString =>
            this.UpdateTime.ToShortDateString() + " " +
            this.UpdateTime.ToShortTimeString();
        /// <summary>
        /// 新文件名。
        /// </summary>
        public abstract string NewName { get; set; }
        /// <summary>
        /// 文件重命名后的新路径。
        /// </summary>
        public string NewFullName => Path.Combine(this.Directory, this.NewName);
        /// <summary>
        /// 指示文件重命名是否完成。
        /// </summary>
        public abstract bool RenameCompleted { get; set; }

        /// <summary>
        /// 在属性更改时发生。
        /// </summary>
        public abstract event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 使用文件路径创建此类的实例。
        /// </summary>
        /// <param name="filePath">文件路径，无法打开则创建一个占位符。</param>
        /// <returns>一个 <see cref="FileRenameInfo"/> 类的实例。</returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static FileRenameInfo Create(string filePath) =>
            BindableTypeProvider<FileRenameInfo>.Default.CreateInstance(filePath);
   }
}
