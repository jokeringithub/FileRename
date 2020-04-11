using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace XstarS.FileRename.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 初始化 <see cref="MainWindow"/> 类。
        /// </summary>
        static MainWindow()
        {
            MainWindow.InitializeCommandBindings();
        }

        /// <summary>
        /// 初始化 <see cref="MainWindow"/> 的实例。
        /// </summary>
        public MainWindow()
        {
            this.DataContext = new MainWindowModel();
            this.Model.PropertyChanged += this.Model_PropertyChanged;
            this.Model.ErrorsChanged += this.Model_ErrorsChanged;
            this.InitializeComponent();
        }

        /// <summary>
        /// 当前 <see cref="MainWindow"/> 的数据逻辑模型。
        /// </summary>
        public MainWindowModel Model => (MainWindowModel)this.DataContext;

        /// <summary>
        /// 初始化 <see cref="MainWindow"/> 的命令绑定。
        /// </summary>
        private static void InitializeCommandBindings()
        {
            var commandBindings = new[]
            {
                new CommandBinding(MainWindow.AddNamingRuleCommand,
                    (sender, e) => ((MainWindow)sender).Model.AddNamingRule()),
                new CommandBinding(MainWindow.RemoveNamingRuleCommand,
                    (sender, e) => ((MainWindow)sender).Model.RemoveNamingRule(),
                    (sender, e) => e.CanExecute = ((MainWindow)sender).Model.HasSelectedNamingRule),
                new CommandBinding(MainWindow.AddDummyFileCommand,
                    (sender, e) => ((MainWindow)sender).Model.AddDummyFile()),
                new CommandBinding(MainWindow.AddRenamingFileCommand,
                    (sender, e) => ((MainWindow)sender).AddRenamingFile()),
                new CommandBinding(MainWindow.RemoveRenamingFileCommand,
                    (sender, e) => ((MainWindow)sender).Model.RemoveRenamingFile(),
                    (sender, e) => e.CanExecute = ((MainWindow)sender).Model.HasSelectedRenamingFile),
                new CommandBinding(MainWindow.ClearRenamingFilesCommand,
                    (sender, e) => ((MainWindow)sender).Model.ClearRenamingFiles(),
                    (sender, e) => e.CanExecute = ((MainWindow)sender).Model.HasRenamingFiles),
                new CommandBinding(MainWindow.PreviewFileRenameCommand,
                    (sender, e) => ((MainWindow)sender).Model.PreviewFileRename(),
                    (sender, e) => e.CanExecute = ((MainWindow)sender).Model.HasRenamingFiles),
                new CommandBinding(MainWindow.DoFileRenameCommand,
                    (sender, e) => ((MainWindow)sender).Model.DoFileRename(),
                    (sender, e) => e.CanExecute = ((MainWindow)sender).Model.CanDoFileRename),
                new CommandBinding(MainWindow.UndoFileRenameCommand,
                    (sender, e) => ((MainWindow)sender).Model.UndoFileRename(),
                    (sender, e) => e.CanExecute = ((MainWindow)sender).Model.CanUndoFileRename)
            };

            foreach (var commandBinding in commandBindings)
            {
                CommandManager.RegisterClassCommandBinding(typeof(MainWindow), commandBinding);
            }
        }

        /// <summary>
        /// 获取表示添加命名规则的命令。
        /// </summary>
        public static RoutedUICommand AddNamingRuleCommand { get; } =
            new RoutedUICommand(
                nameof(MainWindow.AddNamingRuleCommand),
                nameof(MainWindow.AddNamingRuleCommand),
                typeof(MainWindow));

        /// <summary>
        /// 获取表示移除命名规则的命令。
        /// </summary>
        public static RoutedUICommand RemoveNamingRuleCommand { get; } =
            new RoutedUICommand(
                nameof(MainWindow.RemoveNamingRuleCommand),
                nameof(MainWindow.RemoveNamingRuleCommand),
                typeof(MainWindow));

        /// <summary>
        /// 获取表示添加占位文件的命令。
        /// </summary>
        public static RoutedUICommand AddDummyFileCommand { get; } =
            new RoutedUICommand(
                nameof(MainWindow.AddDummyFileCommand),
                nameof(MainWindow.AddDummyFileCommand),
                typeof(MainWindow));

        /// <summary>
        /// 获取表示添加要重命名的文件的命令。
        /// </summary>
        public static RoutedUICommand AddRenamingFileCommand { get; } =
            new RoutedUICommand(
                nameof(MainWindow.AddRenamingFileCommand),
                nameof(MainWindow.AddRenamingFileCommand),
                typeof(MainWindow));

        /// <summary>
        /// 获取表示移除要重命名的文件的命令。
        /// </summary>
        public static RoutedUICommand RemoveRenamingFileCommand { get; } =
            new RoutedUICommand(
                nameof(MainWindow.RemoveRenamingFileCommand),
                nameof(MainWindow.RemoveRenamingFileCommand),
                typeof(MainWindow));

        /// <summary>
        /// 获取表示移除所有要重命名的文件的命令。
        /// </summary>
        public static RoutedUICommand ClearRenamingFilesCommand { get; } =
            new RoutedUICommand(
                nameof(MainWindow.ClearRenamingFilesCommand),
                nameof(MainWindow.ClearRenamingFilesCommand),
                typeof(MainWindow));

        /// <summary>
        /// 获取表示预览新文件名的命令。
        /// 默认键笔势：RCtrl。
        /// </summary>
        public static RoutedUICommand PreviewFileRenameCommand { get; } =
            new RoutedUICommand(
                nameof(MainWindow.PreviewFileRenameCommand),
                nameof(MainWindow.PreviewFileRenameCommand),
                typeof(MainWindow),
                new InputGestureCollection() {
                    new KeyGesture(Key.RightCtrl, ModifierKeys.None, "RCtrl") });

        /// <summary>
        /// 获取表示执行文件重命名的命令。
        /// 默认键笔势：Enter。
        /// </summary>
        public static RoutedUICommand DoFileRenameCommand { get; } =
            new RoutedUICommand(
                nameof(MainWindow.DoFileRenameCommand),
                nameof(MainWindow.DoFileRenameCommand),
                typeof(MainWindow));

        /// <summary>
        /// 获取表示撤销文件重命名的命令。
        /// 默认键笔势：Ctrl+Z。
        /// </summary>
        public static RoutedUICommand UndoFileRenameCommand { get; } =
            new RoutedUICommand(
                nameof(MainWindow.UndoFileRenameCommand),
                nameof(MainWindow.UndoFileRenameCommand),
                typeof(MainWindow),
                new InputGestureCollection() {
                    new KeyGesture(Key.Z, ModifierKeys.Control, "Ctrl+Z") });

        /// <summary>
        /// 创建打开文件对话框，添加要重命名的文件。
        /// </summary>
        private void AddRenamingFile()
        {
            var openDialog = new OpenFileDialog { Multiselect = true };
            if (openDialog.ShowDialog() == true)
            {
                this.Model.AddRenamingFiles(openDialog.FileNames);
            }
        }

        /// <summary>
        /// <see cref="MainWindow.RenamingFileDataGrid"/> 拖拽进入的事件处理。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">提供事件数据的对象。</param>
        private void RenamingFileDataGrid_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[])
            {
                e.Effects = DragDropEffects.Copy;
            }
        }

        /// <summary>
        /// <see cref="MainWindow.RenamingFileDataGrid"/> 拖拽放下的事件处理。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">提供事件数据的对象。</param>
        private void RenamingFileDataGrid_Drop(object sender, DragEventArgs e)
        {
            string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
            this.Model.AddRenamingFiles(paths);
        }

        /// <summary>
        /// 当前窗口的数据逻辑模型的属性发生改变的事件处理。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">提供事件数据的对象。</param>
        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
        }

        /// <summary>
        /// 当前窗口数据逻辑模型的验证错误更改的事件处理。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">提供事件数据的对象。</param>
        private void Model_ErrorsChanged(object sender, DataErrorsChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName))
            {
                var errors = ((MainWindowModel)sender).GetErrors(e.PropertyName);
                if (!(errors is null))
                {
                    var message = string.Join(Environment.NewLine, errors.OfType<object>());
                    MessageBox.Show(this, message,
                        "异常", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}
