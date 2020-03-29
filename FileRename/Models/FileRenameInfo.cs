using System;
using System.Collections.Generic;
using System.IO;
using XstarS.ComponentModel;
using mstring = System.Text.StringBuilder;

namespace XstarS.FileRename.Models
{
    /// <summary>
    /// 提供与文件重命名有关的文件信息。
    /// </summary>
    public class FileRenameInfo : ObservableDataObject
    {
        /// <summary>
        /// 表示占位符文件的文件路径。
        /// </summary>
        public static readonly string DummyFilePath = Path.GetTempFileName();

        /// <summary>
        /// 要重命名的文件的文件信息。
        /// </summary>
        private readonly FileInfo _File;

        /// <summary>
        /// 使用占位符文件初始化 <see cref="FileRenameInfo"/> 的新实例。
        /// </summary>
        public FileRenameInfo() : this(FileRenameInfo.DummyFilePath)
        {
            this.IsSelected = false;
        }

        /// <summary>
        /// 使用文件路径初始化 <see cref="FileRenameInfo"/> 类的新实例。
        /// </summary>
        /// <param name="filePath">要重命名的文件的路径。</param>
        public FileRenameInfo(string filePath)
        {
            this.IsSelected = true;
            this.IsRenamed = false;
            this._File = new FileInfo(filePath);
        }

        /// <summary>
        /// 获取文件是否被选中。
        /// </summary>
        public bool IsSelected
        {
            get => this.GetProperty<bool>();
            set => this.SetProperty(value);
        }

        /// <summary>
        /// 获取文件名。
        /// </summary>
        public string Name => this._File.Name;

        /// <summary>
        /// 获取文件扩展名。
        /// </summary>
        public string Extension => this._File.Extension;

        /// <summary>
        /// 获取不带扩展名的文件名。
        /// </summary>
        public string NameWithoutExtension =>
            Path.GetFileNameWithoutExtension(this.Name);

        /// <summary>
        /// 获取文件所在目录名称。
        /// </summary>
        public string DirectoryName => this._File.DirectoryName;

        /// <summary>
        /// 获取文件完整路径。
        /// </summary>
        public string FullName => this._File.FullName;

        /// <summary>
        /// 获取以字节为单位的文件大小。
        /// </summary>
        public long Length => this._File.Length;

        /// <summary>
        /// 获取以 KB, MB, GB 等为单位的文件大小。
        /// </summary>
        public string Size =>
            (this.Length < Math.Pow(1024, 2)) ?
            (this.Length / Math.Pow(1024, 1)).ToString("F2") + " KB" :
            (this.Length < Math.Pow(1024, 3)) ?
            (this.Length / Math.Pow(1024, 2)).ToString("F2") + " MB" :
            (this.Length < Math.Pow(1024, 4)) ?
            (this.Length / Math.Pow(1024, 3)).ToString("F2") + " GB" :
            (this.Length / Math.Pow(1024, 4)).ToString("F2") + " TB";

        /// <summary>
        /// 获取文件修改日期。
        /// </summary>
        public DateTime LastWriteTime => this._File.LastWriteTime;

        /// <summary>
        /// 获取当前文件是否为占位符文件。
        /// </summary>
        public bool IsDummy => this.FullName == FileRenameInfo.DummyFilePath;

        /// <summary>
        /// 获取新文件名。
        /// </summary>
        public string NewName
        {
            get => this.GetProperty<string>();
            private set => this.SetProperty(value);
        }

        /// <summary>
        /// 获取新文件的完整路径。
        /// </summary>
        public string NewFullName =>
            Path.Combine(this.DirectoryName, this.NewName);

        /// <summary>
        /// 获取文件重命名是否完成。
        /// </summary>
        public bool IsRenamed
        {
            get => this.GetProperty<bool>();
            private set => this.SetProperty(value);
        }

        /// <summary>
        /// 获取当前是否可以执行文件重命名。
        /// </summary>
        public bool CanDoRename =>
            this.IsSelected && !this.IsDummy && !(this.NewName is null) && !this.IsRenamed;

        /// <summary>
        /// 获取当前是否可以撤销文件重命名。
        /// </summary>
        public bool CanUndoRename => this.IsSelected && !this.IsDummy && this.IsRenamed;

        /// <summary>
        /// 根据指定的命名规则列表和当前文件的序号更新新文件名。
        /// </summary>
        /// <param name="rules">命名规则列表。</param>
        /// <param name="index">当前文件的序号。</param>
        /// <exception cref="Exception">生成新文件名过程中出错。</exception>
        public void UpdateNewName(IList<NamingRule> rules, int index)
        {
            if (this.IsDummy) { return; }
            var newName = new mstring();
            foreach (var rule in rules)
            {
                newName.Append(rule.GetName(this, index));
            }
            this.NewName = newName.ToString();
        }

        /// <summary>
        /// 执行文件重命名。
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design", "CA1031:DoNotNatchGeneralExceptionTypes")]
        public void DoRename()
        {
            if (this.CanDoRename)
            {
                try
                {
                    File.Move(this.FullName, this.NewFullName);
                    this.IsRenamed = true;
                }
                catch (Exception) { }
            }
        }

        /// <summary>
        /// 撤销文件重命名。
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design", "CA1031:DoNotNatchGeneralExceptionTypes")]
        public void UndoRename()
        {
            if (this.CanUndoRename)
            {
                try
                {
                    File.Move(this.NewFullName, this.FullName);
                    this.IsRenamed = false;
                }
                catch (Exception) { }
            }
        }
    }
}
