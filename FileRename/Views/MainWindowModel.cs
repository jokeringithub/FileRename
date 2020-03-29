using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using XstarS.ComponentModel;
using XstarS.FileRename.Helpers;
using XstarS.FileRename.Models;

namespace XstarS.FileRename.Views
{
    /// <summary>
    /// 表示 <see cref="MainWindow"/> 的数据模型。
    /// </summary>
    public class MainWindowModel : ObservableDataObject
    {
        /// <summary>
        /// 初始化 <see cref="MainWindowModel"/> 类的新实例。
        /// </summary>
        public MainWindowModel()
        {
            this.NamingRules = new ObservableCollection<NamingRule>();
            this.RenamingFiles = new ObservableCollection<FileRenameInfo>();
            this.RenamingFiles.CollectionChanged += this.RenamingFiles_CollectionChanged;
            this.SelectedNamingRuleIndex = 0;
            this.SelectedRenamingFileIndex = -1;
            this.InitializeNamingRules();
        }

        /// <summary>
        /// 获取命名规则列表。
        /// </summary>
        public ObservableCollection<NamingRule> NamingRules { get; }

        /// <summary>
        /// 获取或设置当前选中的命名规则的索引。
        /// </summary>
        public int SelectedNamingRuleIndex
        {
            get => this.GetProperty<int>();
            set
            {
                this.SetProperty(value);
                this.NotifyPropertyChanged(nameof(this.CanRemoveNamingRule));
            }
        }

        /// <summary>
        /// 获取当前是否可以移除命名规则。
        /// </summary>
        public bool CanRemoveNamingRule => this.SelectedNamingRuleIndex >= 0;

        /// <summary>
        /// 获取要重命名的文件。
        /// </summary>
        public ObservableCollection<FileRenameInfo> RenamingFiles { get; }

        /// <summary>
        /// 获取或设置当前选中的要重命名的文件的索引。
        /// </summary>
        public int SelectedRenamingFileIndex
        {
            get => this.GetProperty<int>();
            set
            {
                this.SetProperty(value);
                this.NotifyPropertyChanged(nameof(this.CanRemoveRenamingFile));
            }
        }

        /// <summary>
        /// 获取当前是否可以移除要重命名的文件。
        /// </summary>
        public bool CanRemoveRenamingFile => this.SelectedRenamingFileIndex >= 0;

        /// <summary>
        /// 获取当前是否可以移除所有要重命名的文件。
        /// </summary>
        public bool CanClearRenamingFiles => this.RenamingFiles.Count > 0;

        /// <summary>
        /// 获取当前是否可以执行文件重命名。
        /// </summary>
        public bool CanDoFileRename =>
            this.RenamingFiles.Any(file => file.IsSelected && file.CanDoRename);

        /// <summary>
        /// 获取当前是否可以撤销文件重命名。
        /// </summary>
        public bool CanUndoFileRename =>
            this.RenamingFiles.Any(file => file.IsSelected && file.CanUndoRename);

        /// <summary>
        /// 初始化命名规则列表。
        /// </summary>
        private void InitializeNamingRules()
        {
            this.NamingRules.Add(new NamingRule(NamingRuleType.OrderedNumber) { NumberLength = 3 });
            this.NamingRules.Add(new NamingRule(NamingRuleType.Extension));
        }

        /// <summary>
        /// 在命名规则列表中指定位置之后添加一条新的命名规则。
        /// </summary>
        public void AddNamingRule()
        {
            this.NamingRules.Insert(this.SelectedNamingRuleIndex + 1,
                new NamingRule(NamingRuleType.ConstantString));
            this.SelectedNamingRuleIndex++;
        }

        /// <summary>
        /// 移除命名规则列表中指定位置的命名规则。
        /// </summary>
        public void RemoveNamingRule()
        {
            this.NamingRules.RemoveAt(this.SelectedNamingRuleIndex);
            this.SelectedNamingRuleIndex--;
        }

        /// <summary>
        /// 在文件列表中指定位置之后添加一个占位符文件。
        /// </summary>
        public void AddDummyFile()
        {
            this.RenamingFiles.Insert(this.SelectedRenamingFileIndex + 1, new FileRenameInfo());
            this.SelectedRenamingFileIndex++;
        }

        /// <summary>
        /// 添加指定路径的文件到文件列表。
        /// </summary>
        /// <param name="path">要添加到文件列表的文件路径。</param>
        public void AddRenamingFile(string path)
        {
            foreach (var filePath in PathHelper.GetFilePaths(path))
            {
                this.RenamingFiles.Add(new FileRenameInfo(filePath));
            }
        }

        /// <summary>
        /// 添加多个指定路径的文件到文件列表。
        /// </summary>
        /// <param name="paths">要添加到文件列表的多个文件路径。</param>
        public void AddRenamingFiles(string[] paths)
        {
            foreach (var path in paths)
            {
                this.AddRenamingFile(path);
            }
        }

        /// <summary>
        /// 移除文件列表中指定位置的文件。
        /// </summary>
        public void RemoveRenamingFile()
        {
            this.RenamingFiles.RemoveAt(this.SelectedRenamingFileIndex);
            this.SelectedRenamingFileIndex--;
        }

        /// <summary>
        /// 移除文件列表中的所有文件。
        /// </summary>
        public void ClearRenamingFiles()
        {
            this.RenamingFiles.Clear();
            this.SelectedRenamingFileIndex = -1;
        }

        /// <summary>
        /// 根据当前命名规则生成新文件名。
        /// </summary>
        public void PreviewFileRename()
        {
            for (int index = 0; index < this.RenamingFiles.Count; index++)
            {
                var file = this.RenamingFiles[index];
                file.UpdateNewName(this.NamingRules, index);
            }
            this.NotifyPropertyChanged(nameof(this.CanDoFileRename));
            this.NotifyPropertyChanged(nameof(this.CanUndoFileRename));
        }

        /// <summary>
        /// 执行文件重命名。
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// 在未应用新文件名的情况下执行文件重命名。</exception>
        public void DoFileRename()
        {
            while (true)
            {
                var changed = false;
                foreach (var file in this.RenamingFiles)
                {
                    if (file.CanDoRename)
                    {
                        file.DoRename();
                        if (file.IsRenamed)
                        {
                            changed = true;
                        }
                    }
                }
                if (!changed) { break; }
            }
            this.NotifyPropertyChanged(nameof(this.CanDoFileRename));
            this.NotifyPropertyChanged(nameof(this.CanUndoFileRename));
        }

        /// <summary>
        /// 撤销文件重命名。
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// 在未执行文件重命名的情况下撤销文件重命名。</exception>
        public void UndoFileRename()
        {
            while (true)
            {
                var changed = false;
                foreach (var file in this.RenamingFiles)
                {
                    if (file.CanUndoRename)
                    {
                        file.UndoRename();
                        if (!file.IsRenamed)
                        {
                            changed = true;
                        }
                    }
                }
                if (!changed) { break; }
            }
            this.NotifyPropertyChanged(nameof(this.CanDoFileRename));
            this.NotifyPropertyChanged(nameof(this.CanUndoFileRename));
        }

        /// <summary>
        /// <see cref="MainWindowModel.RenamingFiles"/> 集合发生更改时的事件处理。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">提供事件数据的对象。</param>
        private void RenamingFiles_CollectionChanged(
            object sender, NotifyCollectionChangedEventArgs e)
        {
            this.NotifyPropertyChanged(nameof(this.CanClearRenamingFiles));
            this.NotifyPropertyChanged(nameof(this.CanDoFileRename));
            this.NotifyPropertyChanged(nameof(this.CanUndoFileRename));
        }
    }
}
