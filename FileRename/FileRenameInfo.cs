using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XstarS.ComponentModel;

namespace FileRename
{
    /// <summary>
    /// 文件信息类，仅提供与文件重命名有关的文件信息
    /// </summary>
    public class FileRenameInfo : BindableObject, IEquatable<FileRenameInfo>
    {
        /// <summary>
        /// 指示是否被选中。
        /// </summary>
        private bool selected;
        /// <summary>
        /// 新文件名。
        /// </summary>
        private string newName;
        /// <summary>
        /// 指示文件重命名是否完成。
        /// </summary>
        private bool renameCompleted;

        /// <summary>
        /// 使用 <see cref="FileInfo"/> 实例初始化此类。
        /// </summary>
        /// <param name="fileInfo"><see cref="FileInfo"/> 实例。</param>
        public FileRenameInfo(FileInfo fileInfo)
        {
            this.selected = true;
            this.Name = fileInfo.Name;
            this.Extension = fileInfo.Extension;
            this.FullName = fileInfo.FullName;
            this.Directory = fileInfo.DirectoryName;
            this.renameCompleted = false;
            this.Size = fileInfo.Length;
            this.UpdateTime = fileInfo.LastWriteTime;
        }

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
                this.selected = true;
                this.Name = fileInfo.Name;
                this.Extension = fileInfo.Extension;
                this.FullName = fileInfo.FullName;
                this.Directory = fileInfo.DirectoryName;
                this.renameCompleted = false;
                this.Size = fileInfo.Length;
                this.UpdateTime = fileInfo.LastWriteTime;
            }
            catch (Exception)
            {
                this.selected = true;
                this.Name = "***";
                this.Extension = string.Empty;
                this.FullName = string.Empty;
                this.Directory = string.Empty;
                this.renameCompleted = false;
                this.Size = 0;
                this.UpdateTime = new DateTime();
            }
        }

        /// <summary>
        /// 指示是否被选中。
        /// </summary>
        public bool Selected
        {
            get => this.selected;
            set => this.SetProperty(ref this.selected, value);
        }
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
        public string SizeString
        {
            get
            {
                if (Size < Math.Pow(1024, 2))
                {
                    return Math.Round((Size / Math.Pow(1024, 1)), 2).ToString() + " KB";
                }
                else if (Size < Math.Pow(1024, 3))
                {
                    return Math.Round((Size / Math.Pow(1024, 2)), 2).ToString() + " MB";
                }
                else if (Size < Math.Pow(1024, 4))
                {
                    return Math.Round((Size / Math.Pow(1024, 3)), 2).ToString() + " GB";
                }
                else
                {
                    return Math.Round((Size / Math.Pow(1024, 4)), 2).ToString() + " TB";
                }
            }
        }
        /// <summary>
        /// 文件修改日期。
        /// </summary>
        public DateTime UpdateTime { get; }
        /// <summary>
        /// 文件修改日期字符串，格式为 "短日期 短时间"。
        /// </summary>
        public string UpdateTimeString
        {
            get =>
                this.UpdateTime.ToShortDateString() +
                " " +
                this.UpdateTime.ToShortTimeString();
        }
        /// <summary>
        /// 新文件名。
        /// </summary>
        public string NewName { get => this.newName; set => this.SetProperty(ref this.newName, value); }
        /// <summary>
        /// 文件重命名后的新路径。
        /// </summary>
        public string NewFullName { get => this.Directory + @"\" + this.newName; }
        /// <summary>
        /// 指示文件重命名是否完成。
        /// </summary>
        public bool RenameCompleted { get => this.renameCompleted; set => this.SetProperty(ref this.renameCompleted, value); }

        /// <summary>
        /// 判断是否为同一文件。
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(FileRenameInfo other) =>
            StringComparer.CurrentCultureIgnoreCase.Equals(this.FullName, other.FullName);

        /// <summary>
        /// 判断是否为同一文件。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj) =>
            (obj is FileRenameInfo fileRenameInfo) && this.Equals(fileRenameInfo);

        /// <summary>
        /// 生成基于文件路径的散列值。
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => 
            -1223983128 + StringComparer.CurrentCultureIgnoreCase.GetHashCode(FullName);
   }
}
