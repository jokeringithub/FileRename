/// <summary>
/// 获取给定条件下所有文件的绝对路径的方法
/// </summary>

namespace System.IO
{
    /// <summary>
    /// 获取给定条件下所有文件的绝对路径的静态类，此类不可被实例化
    /// </summary>
    public abstract class AllFilePaths
    {
        /// <summary>
        /// 获取当前工作目录及其子目录中包含的所有文件的绝对路径的方法
        /// </summary>
        /// <returns>所有文件的绝对路径字符串数组</returns>
        public static string[] GetAllFilePathsInCurrentDirectory() => GetAllFilePaths(Environment.CurrentDirectory);

        /// <summary>
        /// 获取整个文件系统中包含的所有文件的绝对路径的方法
        /// </summary>
        /// <returns>所有文件的绝对路径字符串数组</returns>
        public static string[] GetAllFilePathsInWholeFileSystem() => GetAllFilePaths(Environment.GetLogicalDrives());

        /// <summary>
        /// 获取文件或目录路径字符串中包含的所有文件及其子目录中包含的所有文件的绝对路径的默认方法（即包含子目录）
        /// </summary>
        /// <param name="fileOrDirectoryPath">输入的文件或目录的绝对路径的字符串</param>
        /// <returns>所有文件的绝对路径字符串数组</returns>
        public static string[] GetAllFilePaths(string fileOrDirectoryPath) => GetAllFilePaths(fileOrDirectoryPath, true);

        /// <summary>
        /// 获取字符串数组中包含的所有文件的绝对路径和所有目录及其子目录中包含的所有文件的绝对路径的默认方法（即包含子目录）
        /// </summary>
        /// <param name="fileAndDirectoryPaths">输入的文件的绝对路径与目录的绝对路径的字符串数组</param>
        /// <returns>所有文件的绝对路径字符串数组</returns>
        public static string[] GetAllFilePaths(string[] fileAndDirectoryPaths) => GetAllFilePaths(fileAndDirectoryPaths, true);

        /// <summary>
        /// 获取文件或目录路径字符串中包含的所有文件及其子目录中包含的所有文件的绝对路径的方法
        /// </summary>
        /// <param name="fileOrDirectoryPath">输入的文件或目录的绝对路径的字符串</param>
        /// <param name="containSubDirectories">指示是否包含子目录</param>
        /// <returns></returns>
        public static string[] GetAllFilePaths(string fileOrDirectoryPath, bool containSubDirectories) 
            => GetAllFilePaths(new string[] { fileOrDirectoryPath }, containSubDirectories);

        /// <summary>
        /// 获取字符串数组中包含的所有文件的绝对路径和所有目录及其子目录中包含的所有文件的绝对路径的方法
        /// </summary>
        /// <param name="fileAndDirectoryPaths">获取字符串数组中包含的所有文件的绝对路径和所有目录及其子目录中包含的所有文件的绝对路径的方法</param>
        /// <param name="containSubDirectories">指示是否包含子目录</param>
        /// <returns></returns>
        public static string[] GetAllFilePaths(string[] fileAndDirectoryPaths, bool containSubDirectories)
        {
            // 所有文件的绝对路径字符串列表，用于动态添加项目并最终将其复制到一字符串数组
            System.Collections.Generic.List<string> allFilePathList = new System.Collections.Generic.List<string>();

            // 遍历输入的字符串数组
            foreach (string fileAndDirectoryPath in fileAndDirectoryPaths)
            {
                // 若为文件则直接添加
                if (File.Exists(fileAndDirectoryPath))
                {
                    // 获取文件的绝对路径的字符串数组并添加到allFilePathList
                    FileInfo fileInfo = new FileInfo(fileAndDirectoryPath);
                    string filePath = fileInfo.FullName;
                    allFilePathList.Add(filePath);
                }

                // 若为目录则列出内容
                else if (Directory.Exists(fileAndDirectoryPath))
                {
                    // 目录的绝对路径字符串
                    string directoryPath = fileAndDirectoryPath;

                    // 对于内含文件则直接添加
                    try
                    {
                        // 获取所有内含文件的绝对路径的字符串数组并添加到allFilePathList
                        string[] filePaths = Directory.GetFiles(directoryPath);
                        allFilePathList.AddRange(filePaths);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    // 判断是否要包含子目录
                    if (containSubDirectories)
                    {
                        try
                        {
                            // 对于子目录则递归调用并添加返回的文件的绝对路径字符串数组到allFilePathList
                            string[] subDirectoryPaths = Directory.GetDirectories(directoryPath);
                            string[] subDirectoryFilePaths = GetAllFilePaths(subDirectoryPaths, containSubDirectories);
                            allFilePathList.AddRange(subDirectoryFilePaths);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
            }

            // 返回所有文件的绝对路径字符串数组
            return allFilePathList.ToArray();
        }
    }
}
