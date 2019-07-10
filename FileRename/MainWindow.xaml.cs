using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XstarS.ComponentModel;

namespace FileRename
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 初始化 <see cref="MainWindow"/> 的实例。
        /// </summary>
        public MainWindow()
        {
            this.NameRuleList = new BindingList<NameRule>();
            this.FileRenameInfoList = new BindingList<FileRenameInfo>();
            this.RecurseSubdirectories = false;
            this.InitializeParameter();
            this.InitializeComponent();
        }

        /// <summary>
        /// 命名规则列表。
        /// </summary>
        public IList<NameRule> NameRuleList { get; }
        /// <summary>
        /// 文件重命名信息列表。
        /// </summary>
        public IList<FileRenameInfo> FileRenameInfoList { get; }
        /// <summary>
        /// 指示是否包含子目录。
        /// </summary>
        public Bindable<bool> RecurseSubdirectories { get; }

        /// <summary>
        /// 初始化与控件绑定的数据并给定初始重命名规则。
        /// </summary>
        private void InitializeParameter()
        {
            var rule1 = NameRule.Create(NameRule.TypeCode.OrderNumber);
            rule1.NumberLength = 3;
            rule1.StartNumberString = "001";
            rule1.EndNumberString = "999";
            var rule2 = NameRule.Create(NameRule.TypeCode.Extension);
            this.NameRuleList.Add(rule1);
            this.NameRuleList.Add(rule2);
            this.RecurseSubdirectories.Value = false;
        }

        /// <summary>
        /// 在当前规则之后添加新规则。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddRuleButton_Click(object sender, RoutedEventArgs e)
        {
            this.ruleListBox.SelectedIndex = this.AddRule(this.ruleListBox.SelectedIndex);
        }

        /// <summary>
        /// 添加新规则。
        /// </summary>
        /// <param name="index">要插入的位置的前一项的索引。</param>
        /// <returns>插入的新规则的索引。</returns>
        private int AddRule(int index)
        {
            this.NameRuleList.Insert(index + 1, NameRule.Create(NameRule.TypeCode.ConstantString));
            return index + 1;
        }

        /// <summary>
        /// 删除当前所选规则。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteRuleButton_Click(object sender, RoutedEventArgs e)
        {
            this.ruleListBox.SelectedIndex = this.DeleteRule(this.ruleListBox.SelectedIndex);
        }

        /// <summary>
        /// 删除一条规则。
        /// </summary>
        /// <param name="index">要删除的规则的索引。</param>
        /// <returns>要删除的规则的前一项索引。</returns>
        private int DeleteRule(int index)
        {
            // 删除所选的命名规则。
            this.NameRuleList.RemoveAt(index);
            return index - 1;
        }

        /// <summary>
        /// 文件拖放至 <see cref="MainWindow.fileInfoDataGrid"/>。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileInfoDataGrid_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) != null) { e.Effects = DragDropEffects.Copy; }
        }

        /// <summary>
        /// 文件拖放完成，添加信息至 <see cref="MainWindow.FileRenameInfoList"/>。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileInfoDataGrid_Drop(object sender, DragEventArgs e)
        {
            string[] dragedStrings = (string[])e.Data.GetData(DataFormats.FileDrop);
            this.AddFileInfo(dragedStrings);
        }

        /// <summary>
        /// 添加文件信息。
        /// </summary>
        private void AddFileInfo(string[] fileAndDirectoryPaths)
        {
            // 取出拖放文件和目录中文件的路径。
            var filePathList = new List<string>();
            foreach (string path in fileAndDirectoryPaths)
            {
                if (File.Exists(path))
                { filePathList.Add(Path.GetFullPath(path)); }
                else if (Directory.Exists(path))
                {
                    filePathList.AddRange(Directory.EnumerateFiles(path, "*",
                        this.RecurseSubdirectories ?
                        SearchOption.AllDirectories :
                        SearchOption.TopDirectoryOnly));
                }
            }

            // 添加拖放的文件信息。
            foreach (string filePath in filePathList)
            {
                try
                {
                    var fileRenameInfo = FileRenameInfo.Create(filePath);
                    // 判断是否已经存在。
                    if (!this.FileRenameInfoList.Contains(fileRenameInfo))
                    { this.FileRenameInfoList.Add(fileRenameInfo); }
                }
                catch (Exception) { continue; }
            }
        }

        /// <summary>
        /// 清空文件列表。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileListClearButton_Click(object sender, RoutedEventArgs e)
        {
            this.FileRenameInfoList.Clear();
        }

        /// <summary>
        /// 按下执行重命名按钮。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            this.RunFileRename();
        }

        /// <summary>
        /// 执行重命名。
        /// </summary>
        private void RunFileRename()
        {
            // 若命名规则最后一项不为扩展名则警告，并提示撤销操作。
            if (this.NameRuleList.Last().RuleType != NameRule.TypeCode.Extension)
            {
                var result = MessageBox.Show(this, "文件命名规则不以扩展名结尾。", "警告", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Cancel) { return; }
            }

            // 若所有文件都没有应用新文件名则退出。
            if (this.FileRenameInfoList.All(
                fileRenameInfo => (fileRenameInfo.NewName == null) || (fileRenameInfo.NewName == string.Empty)))
            {
                MessageBox.Show(this, "请先应用新文件名。", "错误", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 进行重命名。
            this.FileRename();

            // 如果所有选中的文件重命名都已经完成则弹窗提示。
            if (this.FileRenameInfoList.All(fileRenameInfo =>
            {
                // 若被选中文件重命名已完成。
                if (fileRenameInfo.Selected && fileRenameInfo.RenameCompleted) { return true; }
                // 或新文件名为空。
                else if ((fileRenameInfo.NewName == null) || (fileRenameInfo.NewName == string.Empty)) { return true; }
                // 或者为一占位符。
                else if (fileRenameInfo.FullName == string.Empty) { return true; }
                // 否则认为重命名未完成。
                else { return false; }
            }))
            {
                var result = MessageBox.Show(this, "文件重命名完成。", "完成", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                if (result == MessageBoxResult.Cancel) { this.RollbackFileRename(); }
            }
            // 否则提示撤销操作。
            else
            {
                var result = MessageBox.Show(this, "文件重命名未完成，是否撤销重命名？", "错误", MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (result == MessageBoxResult.Yes) { this.RollbackFileRename(); }
            }
        }

        /// <summary>
        /// 文件重命名任务。
        /// </summary>
        private void FileRename()
        {
            // 文件成功重命名计数。
            int fileRenameSuccessCount = 0;
            int fileRenameLastSuccessCount = -1;
            // 循环直至可能的文件重命名全部完成。
            while (true)
            {
                // 检测是否当前完成重命名文件的数量。
                fileRenameSuccessCount = 0;
                foreach (var fileRenameInfo in this.FileRenameInfoList)
                { if (fileRenameInfo.RenameCompleted) { fileRenameSuccessCount++; } }

                // 判断两次循环之间成功命名的文件数量是否增加。
                if (fileRenameSuccessCount != fileRenameLastSuccessCount)
                { fileRenameLastSuccessCount = fileRenameSuccessCount; }
                // 若没有增加则认为可能的文件重命名已经全部完成。
                else { break; }

                // 进行一次循环重命名。
                foreach (var fileRenameInfo in this.FileRenameInfoList)
                {
                    try
                    {
                        var fileInfo = new FileInfo(fileRenameInfo.FullName);
                        if (fileRenameInfo.Selected) { fileInfo.MoveTo(fileRenameInfo.NewFullName); }
                        fileRenameInfo.RenameCompleted = true;
                    }
                    catch (Exception) { continue; }
                }
            }
        }

        /// <summary>
        /// 撤销文件重命名任务。
        /// </summary>
        private void RollbackFileRename()
        {
            int fileRenameRollBackSuccessCount = 0;
            int fileRenameRollBackLastSuccessCount = -1;
            while (true)
            {
                // 检测文件撤销重命名完成的数量。
                fileRenameRollBackSuccessCount = 0;
                foreach (var fileRenameInfo in this.FileRenameInfoList)
                { if (!fileRenameInfo.RenameCompleted) { fileRenameRollBackSuccessCount++; } }

                // 判断两次循环之间成功撤销重命名的文件数量是否增加。
                if (fileRenameRollBackSuccessCount != fileRenameRollBackLastSuccessCount)
                { fileRenameRollBackLastSuccessCount = fileRenameRollBackSuccessCount; }
                // 若没有增加则认为可能的文件恢复命名已经全部完成。
                else { break; }

                // 进行一次循环撤销重命名。
                foreach (var fileRenameInfo in this.FileRenameInfoList)
                {
                    try
                    {
                        var fileInfo = new FileInfo(fileRenameInfo.NewFullName);
                        fileInfo.MoveTo(fileRenameInfo.FullName);
                        fileRenameInfo.RenameCompleted = false;
                    }
                    catch (Exception) { continue; }
                }
            }

            // 如果所有文件的撤销重命名都已经完成则弹窗提示。
            if (this.FileRenameInfoList.All(fileBasicInfo => !fileBasicInfo.RenameCompleted))
            { MessageBox.Show(this, "文件撤销重命名完成。", "完成", MessageBoxButton.OK, MessageBoxImage.Information); }
            else
            { MessageBox.Show(this, "文件撤销重命名未完成。", "错误", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        /// <summary>
        /// 浏览文件按钮按下，传递文件名。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Multiselect = true };
            if (openFileDialog.ShowDialog() == true)
            { this.AddFileInfo(openFileDialog.FileNames); }
        }

        /// <summary>
        /// 应用按钮按下。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            this.ApplyFileNewName();
        }

        /// <summary>
        /// 应用新文件名。
        /// </summary>
        private void ApplyFileNewName()
        {
            for (int i = 0; i < this.FileRenameInfoList.Count; i++)
            {
                var fileNewName = new FileNewName(
                    this.NameRuleList, this.FileRenameInfoList[i].FullName, i);
                try
                {
                    this.FileRenameInfoList[i].NewName = fileNewName.Value;
                }
                catch (FormatException)
                {
                    MessageBox.Show(this, "数字输入有误。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                }
                catch (OverflowException)
                {
                    MessageBox.Show(this, "数字超过最大限制。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                }
                catch (ArgumentOutOfRangeException)
                {
                    MessageBox.Show(this, "文件名索引数有误。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                }
                catch (FileNotFoundException)
                {
                    MessageBox.Show(this, "无法计算文件散列值。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }
            }
        }

        /// <summary>
        /// 按下删除按钮，删除所选文件信息。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileInfoDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            this.fileInfoDataGrid.SelectedIndex = this.DeleteFileInfo();
        }

        /// <summary>
        /// 删除所选文件信息。
        /// </summary>
        /// <returns>删除项目第一项的前一项的索引。</returns>
        private int DeleteFileInfo()
        {
            int selectedIndex = fileInfoDataGrid.SelectedIndex;

            // 删除所选的所有条目。
            foreach (var fileRenameInfoObject in new System.Collections.ArrayList(fileInfoDataGrid.SelectedItems))
            { this.FileRenameInfoList.Remove((FileRenameInfo)fileRenameInfoObject); }

            return selectedIndex - 1;
        }

        /// <summary>
        /// 添加占位项按下，在当前所选位置之前添加一条占位符。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DummyAddButton_Click(object sender, RoutedEventArgs e)
        {
            this.fileInfoDataGrid.SelectedIndex = this.AddDummyFileInfo(this.fileInfoDataGrid.SelectedIndex);
        }

        /// <summary>
        /// 在索引项处添加一条占位符。
        /// </summary>
        /// <returns>占位项目后一项的索引。</returns>
        private int AddDummyFileInfo(int index)
        {
            this.FileRenameInfoList.Insert((index >= 0) ? index : 0, FileRenameInfo.Create(""));
            return index + 1;
        }

        /// <summary>
        /// 主窗口按键按下的事件处理。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // 按下右 Ctrl，应用新文件名。
            if (e.Key == Key.RightCtrl)
            {
                ApplyFileNewName();
            }
            // 按下 Enter，执行重命名。
            else if (e.Key == Key.Enter)
            {
                RunFileRename();
            }
            // 文件信息框有焦点时，按下 Delete，删除选中项目。
            else if (e.Key == Key.Delete)
            {
                if (fileInfoDataGrid.IsFocused)
                {
                    DeleteFileInfo();
                }
            }
        }
    }
}
