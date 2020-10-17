using System;
using System.ComponentModel.DataAnnotations;
using XstarS.ComponentModel;

namespace XstarS.FileRename.Models
{
    /// <summary>
    /// 表示文件重命名的命名规则。
    /// </summary>
    public class NamingRule : ObservableValidDataObject
    {
        /// <summary>
        /// 以指定的命名规则初始化 <see cref="NamingRule"/> 类的新实例。
        /// </summary>
        /// <param name="type">指定的命名规则。</param>
        public NamingRule(NamingRuleType type)
        {
            this.RuleType = type;
            this.ConstantString = string.Empty;
            this.HashType = new FileHashTypeView() { MD5 = true };
            this.NumberLength = 0;
            this.StartNumber = 1;
            this.StartIndex = 0;
            this.EndIndex = -1;
        }

        /// <summary>
        /// 获取或设置命名规则类型。
        /// </summary>
        public NamingRuleType RuleType
        {
            get => this.GetProperty<NamingRuleType>();
            set => this.SetProperty(value);
        }

        /// <summary>
        /// 获取或设置固定文本的值。
        /// </summary>
        public string ConstantString
        {
            get => this.GetProperty<string>();
            set => this.SetProperty(value);
        }

        /// <summary>
        /// 获取编号数字长度，为 0 表示可变长度。
        /// </summary>
        [Range(0, int.MaxValue)]
        public int NumberLength
        {
            get => this.GetProperty<int>();
            set => this.SetProperty(value);
        }

        /// <summary>
        /// 获取或设置编号起始值。
        /// </summary>
        [Range(0, int.MaxValue)]
        public int StartNumber
        {
            get => this.GetProperty<int>();
            set => this.SetProperty(value);
        }

        /// <summary>
        /// 获取或设置文件哈希值的类型的视图。
        /// </summary>
        public FileHashTypeView HashType
        {
            get => this.GetProperty<FileHashTypeView>();
            set => this.SetProperty(value);
        }

        /// <summary>
        /// 获取或设置原文件名起始索引。
        /// </summary>
        [Range(0, int.MaxValue)]
        public int StartIndex
        {
            get => this.GetProperty<int>();
            set => this.SetProperty(value);
        }

        /// <summary>
        /// 获取或设置原文件名结束索引，为 -1 表示文件名结尾。
        /// </summary>
        [Range(-1, int.MaxValue)]
        public int EndIndex
        {
            get => this.GetProperty<int>();
            set => this.SetProperty(value);
        }

        /// <summary>
        /// 获取或设置是否使用自定义文件扩展名。
        /// </summary>
        public bool IsCustomExtension
        {
            get => this.GetProperty<bool>();
            set => this.SetProperty(value);
        }

        /// <summary>
        /// 获取或设置自定义文件扩展名。
        /// </summary>
        public string CustomExtension
        {
            get => this.GetProperty<string>();
            set => this.SetProperty(value);
        }

        /// <summary>
        /// 获取指定文件按照当前命名规则得到的新文件名的部分。
        /// </summary>
        /// <param name="file">指定文件的重命名信息。</param>
        /// <param name="index">指定文件的序号。</param>
        /// <returns>指定文件按照当前命名规则得到的新文件名的部分。</returns>
        /// <exception cref="Exception">生成新文件名过程中出现错误。</exception>
        public string GetName(FileRenameInfo file, int index)
        {
            if (this.HasErrors)
            {
                throw new ArgumentOutOfRangeException();
            }
            switch (this.RuleType)
            {
                case NamingRuleType.ConstantString:
                    return this.GetConstanString();
                case NamingRuleType.OrderedNumber:
                    return this.GetOrderNumber(index);
                case NamingRuleType.FileHash:
                    return this.GetFileHash(file.FullName);
                case NamingRuleType.FileName:
                    return this.GetFileName(file.NameWithoutExtension);
                case NamingRuleType.Extension:
                    return this.GetExtension(file.Extension);
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 获取固定文本部分。
        /// </summary>
        /// <returns>固定文本部分。</returns>
        private string GetConstanString()
        {
            return !string.IsNullOrEmpty(this.ConstantString) ?
                this.ConstantString : string.Empty;
        }

        /// <summary>
        /// 获取当前顺序编号。
        /// </summary>
        /// <param name="index">当前序号。</param>
        /// <returns>当前顺序编号。</returns>
        private string GetOrderNumber(int index)
        {
            return (this.StartNumber + index).ToString($"D{this.NumberLength}");
        }

        /// <summary>
        /// 获取文件哈希值。
        /// </summary>
        /// <param name="filePath">文件路径。</param>
        /// <returns>文件哈希值。</returns>
        /// <exception cref="Exception">计算文件哈希值时出现错误。</exception>
        private string GetFileHash(string filePath)
        {
            using (var fileHash = new FileHash(filePath, this.HashType.Value))
            {
                fileHash.Compute();
                return fileHash.HashHexString;
            }
        }

        /// <summary>
        /// 获取原文件名的部分内容。
        /// </summary>
        /// <param name="fileName">原文件名。</param>
        /// <returns>原文件名的部分内容。</returns>
        private string GetFileName(string fileName)
        {
            var endIndex = this.EndIndex;
            if (endIndex == -1) { endIndex = fileName.Length; }
            if (endIndex > fileName.Length) { endIndex = fileName.Length; }
            var startIndex = this.StartIndex;
            if (startIndex > endIndex) { startIndex = endIndex; }
            return fileName.Substring(startIndex, endIndex - startIndex);
        }

        /// <summary>
        /// 获取文件扩展名。
        /// </summary>
        /// <param name="extension">原文件扩展名。</param>
        /// <returns>文件扩展名。</returns>
        private string GetExtension(string extension)
        {
            if (this.IsCustomExtension)
            {
                extension = this.CustomExtension.Trim();
                if (!extension.StartsWith("."))
                {
                    extension = $".{extension}";
                }
            }
            return extension;
        }
    }
}
