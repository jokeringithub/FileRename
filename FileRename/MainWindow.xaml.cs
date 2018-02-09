using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FileRename
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 初始化MainWindow窗口类
        /// </summary>
        public MainWindow()
        {
            InitializeParameter();
            InitializeComponent();
        }

        /// <summary>
        /// 命名规则列表
        /// </summary>
        private List<NameRule> nameRuleList;
        /// <summary>
        /// 命名规则列表
        /// </summary>
        public List<NameRule> NameRuleList { get => nameRuleList; set => SetProperty(ref nameRuleList, value); }
        /// <summary>
        /// 文件重命名信息列表
        /// </summary>
        private List<FileRenameInfo> fileRenameInfoList;
        /// <summary>
        /// 文件重命名信息列表
        /// </summary>
        public List<FileRenameInfo> FileRenameInfoList { get => fileRenameInfoList; set => SetProperty(ref fileRenameInfoList, value); }
        /// <summary>
        /// 指示是否包含子目录
        /// </summary>
        private bool containSubDirectories;
        /// <summary>
        /// 指示是否包含子目录
        /// </summary>
        public bool ContainSubDirectories { get => containSubDirectories; set => SetProperty(ref containSubDirectories, value); }

        /// <summary>
        /// 初始化与控件绑定的数据并给定初始重命名规则
        /// </summary>
        private void InitializeParameter()
        {
            nameRuleList = new List<NameRule>();
            fileRenameInfoList = new List<FileRenameInfo>();
            NameRuleList.Add(new NameRule(NameRule.NameRuleType.OrderNumber)
            { NumberLength = 3, StartNumber = "001", EndNumber = "999" });
            NameRuleList.Add(new NameRule(NameRule.NameRuleType.Extension));
        }

        /// <summary>
        /// 在当前规则之后添加新规则
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddRuleButton_Click(object sender, RoutedEventArgs e)
        {
            ruleListBox.SelectedIndex = AddRule(ruleListBox.SelectedIndex);
        }

        /// <summary>
        /// 添加新规则
        /// </summary>
        /// <param name="selectedIndex">要插入的位置的前一项的索引</param>
        /// <returns>插入的新规则的索引</returns>
        private int AddRule(int selectedIndex)
        {
            NameRule newNameRule = new NameRule(NameRule.NameRuleType.ConstantString);
            NameRuleList.Insert(selectedIndex + 1, newNameRule);

            // 以下仅为解决C#代码优化带来的数据不更新问题
            NameRuleList = BindableListUpdate.Update(NameRuleList);

            return selectedIndex + 1;
        }

        /// <summary>
        /// 删除当前所选规则
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteRuleButton_Click(object sender, RoutedEventArgs e)
        {
            ruleListBox.SelectedIndex = DeleteRule(ruleListBox.SelectedIndex);
        }

        /// <summary>
        /// 删除一条规则
        /// </summary>
        /// <param name="selectedIndex">要删除的规则的索引</param>
        /// <returns>要删除的规则的前一项索引</returns>
        private int DeleteRule(int selectedIndex)
        {
            // 删除所选的命名规则
            NameRuleList.RemoveAt(selectedIndex);

            // 以下仅为解决C#代码优化带来的数据不更新问题
            NameRuleList = BindableListUpdate.Update(NameRuleList);

            return selectedIndex - 1;
        }

        /// <summary>
        /// 文件拖放至fileInfoDataGrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileInfoDataGrid_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) != null)
            {
                e.Effects = DragDropEffects.Copy;
            }
        }

        /// <summary>
        /// 文件拖放完成，添加信息至MainWindowUiData.FileBasicInfoList
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileInfoDataGrid_Drop(object sender, DragEventArgs e)
        {
            string[] dragedStrings = (string[])e.Data.GetData(DataFormats.FileDrop);
            AddFileInfo(dragedStrings);
        }

        /// <summary>
        /// 添加文件信息
        /// </summary>
        private void AddFileInfo(string[] fileAndDirectoryPaths)
        {
            // 添加拖放的文件信息
            string[] filePaths = AllFilePaths.GetAllFilePaths(fileAndDirectoryPaths, containSubDirectories);
            foreach (string filePath in filePaths)
            {
                try
                {
                    FileRenameInfo fileRenameInfo = new FileRenameInfo(filePath);
                    // 判断是否已经存在
                    if (!FileRenameInfoList.Contains(fileRenameInfo))
                    {
                        FileRenameInfoList.Add(fileRenameInfo);
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }

            // 以下仅为解决C#代码优化带来的数据不更新问题
            FileRenameInfoList = BindableListUpdate.Update(FileRenameInfoList);
        }

        /// <summary>
        /// 清空文件列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileListClearButton_Click(object sender, RoutedEventArgs e)
        {
            FileRenameInfoList = new List<FileRenameInfo>();
        }

        /// <summary>
        /// 按下执行重命名按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            RunFileRename();
        }

        /// <summary>
        /// 执行重命名
        /// </summary>
        private void RunFileRename()
        {
            // 若命名规则最后一项不为扩展名则警告，并提示撤销操作
            if (NameRuleList.Last().RuleType != NameRule.NameRuleType.Extension)
            {
                var result = MessageBox.Show(this, "文件命名规则不以扩展名结尾。", "警告", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            // 若所有文件都没有应用新文件名则退出
            if (FileRenameInfoList.All(fileBasicInfo => 
            ((fileBasicInfo.FileNewName == null) || (fileBasicInfo.FileNewName == string.Empty))))
            {
                MessageBox.Show(this, "请先应用新文件名。", "错误", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 进行重命名
            FileRename();

            // 如果所有选中的文件重命名都已经完成则弹窗提示
            if (FileRenameInfoList.All(fileBasicInfo =>
                {
                    // 若被选中文件重命名已完成
                    if ((fileBasicInfo.Selected) && (fileBasicInfo.FileRenameCompleted))
                    { return true; }
                    // 或新文件名为空
                    else if ((fileBasicInfo.FileNewName == null) || (fileBasicInfo.FileNewName == string.Empty))
                    { return true; }
                    // 或者为一占位符
                    else if (fileBasicInfo.FilePath == string.Empty)
                    { return true; }
                    // 否则认为重命名未完成
                    else
                    { return false; }
                }))
            {
                var result = MessageBox.Show(this, "文件重命名完成。", "完成", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                if (result == MessageBoxResult.Cancel)
                { RoolBackRenameTask(); }
            }
            // 否则提示撤销操作
            else
            {
                var result = MessageBox.Show(this, "文件重命名未完成，是否撤销重命名？", "错误", MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (result == MessageBoxResult.Yes)
                { RoolBackRenameTask(); }
            }
        }

        /// <summary>
        /// 文件重命名任务
        /// </summary>
        private void FileRename()
        {
            // 文件成功重命名计数
            int fileRenameSuccessCount = 0;
            int fileRenameLastSuccessCount = -1;
            // 循环直至可能的文件重命名全部完成
            while (true)
            {
                // 检测是否当前完成重命名文件的数量
                fileRenameSuccessCount = 0;
                foreach (FileRenameInfo fileBasicInfo in FileRenameInfoList)
                {
                    if (fileBasicInfo.FileRenameCompleted)
                    {
                        fileRenameSuccessCount++;
                    }
                }

                // 判断两次循环之间成功命名的文件数量是否增加
                if (fileRenameSuccessCount != fileRenameLastSuccessCount)
                {
                    fileRenameLastSuccessCount = fileRenameSuccessCount;
                }
                // 若没有增加则认为可能的文件重命名已经全部完成
                else
                {
                    break;
                }

                // 进行一次循环重命名
                foreach (FileRenameInfo fileBasicInfo in FileRenameInfoList)
                {
                    FileInfo fileInfo;
                    try
                    {
                        fileInfo = new FileInfo(fileBasicInfo.FilePath);
                        if (fileBasicInfo.Selected)
                        { fileInfo.MoveTo(fileBasicInfo.FileNewPath); }
                        fileBasicInfo.FileRenameCompleted = true;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// 撤销文件重命名任务
        /// </summary>
        private void RoolBackRenameTask()
        {
            int fileRenameRollBackSuccessCount = 0;
            int fileRenameRollBackLastSuccessCount = -1;
            while (true)
            {
                // 检测文件撤销重命名完成的数量
                fileRenameRollBackSuccessCount = 0;
                foreach (FileRenameInfo fileBasicInfo in FileRenameInfoList)
                {
                    if (!fileBasicInfo.FileRenameCompleted)
                    {
                        fileRenameRollBackSuccessCount++;
                    }
                }

                // 判断两次循环之间成功撤销重命名的文件数量是否增加
                if (fileRenameRollBackSuccessCount != fileRenameRollBackLastSuccessCount)
                {
                    fileRenameRollBackLastSuccessCount = fileRenameRollBackSuccessCount;
                }
                // 若没有增加则认为可能的文件恢复命名已经全部完成
                else
                {
                    break;
                }

                // 进行一次循环撤销重命名
                foreach (FileRenameInfo fileBasicInfo in FileRenameInfoList)
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(fileBasicInfo.FileNewPath);
                        fileInfo.MoveTo(fileBasicInfo.FilePath);
                        fileBasicInfo.FileRenameCompleted = false;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            // 如果所有文件的撤销重命名都已经完成则弹窗提示
            if (FileRenameInfoList.All(fileBasicInfo => !fileBasicInfo.FileRenameCompleted))
            {
                MessageBox.Show(this, "文件撤销重命名完成。", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(this, "文件撤销重命名未完成。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 浏览文件按钮按下，传递文件名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Multiselect = true };
            if (openFileDialog.ShowDialog() == true)
            {
                AddFileInfo(openFileDialog.FileNames);
            }
        }

        /// <summary>
        /// 应用按钮按下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyFileNewName();
        }

        /// <summary>
        /// 应用新文件名
        /// </summary>
        private void ApplyFileNewName()
        {
            for (int i = 0; i < FileRenameInfoList.Count; i++)
            {
                FileNewName fileRename = new FileNewName(NameRuleList, FileRenameInfoList[i].FilePath, i);
                try
                {
                    FileRenameInfoList[i].FileNewName = fileRename.GetNewName();
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

            // 以下仅为解决C#代码优化带来的数据不更新问题
            FileRenameInfoList = BindableListUpdate.Update(FileRenameInfoList);
        }

        /// <summary>
        /// 按下删除按钮，删除所选文件信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileInfoDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            fileInfoDataGrid.SelectedIndex = DeleteFileInfo();
        }

        /// <summary>
        /// 删除所选文件信息
        /// </summary>
        /// <returns>删除项目第一项的前一项的索引</returns>
        private int DeleteFileInfo()
        {
            int selectedIndex = fileInfoDataGrid.SelectedIndex;

            // 删除所选的所有条目
            foreach (var fileBasicInfoObject in fileInfoDataGrid.SelectedItems)
            {
                FileRenameInfoList.Remove((FileRenameInfo)fileBasicInfoObject);
            }

            // 以下仅为解决C#代码优化带来的数据不更新问题
            FileRenameInfoList = BindableListUpdate.Update(FileRenameInfoList);

            return selectedIndex - 1;
        }

        /// <summary>
        /// 添加占位项按下，在当前所选位置之前添加一条占位符
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DummyAddButton_Click(object sender, RoutedEventArgs e)
        {
            fileInfoDataGrid.SelectedIndex = AddDummyFileInfo(fileInfoDataGrid.SelectedIndex);
        }

        /// <summary>
        /// 在索引项处添加一条占位符
        /// </summary>
        /// <returns>占位项目后一项的索引</returns>
        private int AddDummyFileInfo(int index)
        {
            if (index >= 0)
            {
                FileRenameInfoList.Insert(index, new FileRenameInfo(""));
            }
            else
            {
                FileRenameInfoList.Insert(0, new FileRenameInfo(""));
            }

            // 以下仅为解决C#代码优化带来的数据不更新问题
            FileRenameInfoList = BindableListUpdate.Update(FileRenameInfoList);

            return index + 1;
        }

        /// <summary>
        /// 主窗口案件按下的事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // 按下右Ctrl，应用新文件名
            if (e.Key == Key.RightCtrl)
            {
                ApplyFileNewName();
            }
            // 按下Enter，执行重命名
            else if (e.Key == Key.Enter)
            {
                RunFileRename();
            }
            // 文件信息框有焦点时，按下Delete，删除选中项目
            else if (e.Key == Key.Delete)
            {
                if (fileInfoDataGrid.IsFocused)
                {
                    DeleteFileInfo();
                }
            }
        }
    }

    /// <summary>
    /// 此部分用于简易实现INotifyPropertyChanged接口
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        /// <summary>
        /// 属性改变事件，当属性的set使用SetProperty(property, value)实现时触发。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 触发属性改变事件。
        /// </summary>
        /// <param name="propertyName">属性名</param>
        protected void OnPropertyChanged(string propertyName)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// 改变属性时使用的方法。
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="item">属性对应的字段名</param>
        /// <param name="value">填入value</param>
        /// <param name="propertyName">属性名（由编译器自动获取）</param>
        /// <example>
        /// public var Property { get => property; set => SetProperty(property, value); }
        /// </example>
        protected void SetProperty<T>(ref T item, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(item, value))
            {
                item = value;
                OnPropertyChanged(propertyName);
            }
        }
    }
}
