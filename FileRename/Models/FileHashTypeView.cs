using XstarS.ComponentModel;

namespace XstarS.FileRename.Models
{
    /// <summary>
    /// 表示文件哈希值类型 <see cref="FileHashType"/> 的视图。
    /// </summary>
    public sealed class FileHashTypeView : EnumVectorView<FileHashType>
    {
        /// <summary>
        /// 初始化 <see cref="FileHashTypeView"/> 类的实例。
        /// </summary>
        public FileHashTypeView() { }

        /// <summary>
        /// 获取或设置是否计算文件的 MD5 哈希值。
        /// </summary>
        public bool MD5 { get => this.IsEnum(); set => this.SetEnum(value); }

        /// <summary>
        /// 获取或设置是否计算文件的 SHA-1 哈希值。
        /// </summary>
        public bool SHA1 { get => this.IsEnum(); set => this.SetEnum(value); }

        /// <summary>
        /// 获取或设置是否计算文件的 SHA-256 哈希值。
        /// </summary>
        public bool SHA256 { get => this.IsEnum(); set => this.SetEnum(value); }

        /// <summary>
        /// 获取或设置是否计算文件的 SHA-384 哈希值。
        /// </summary>
        public bool SHA384 { get => this.IsEnum(); set => this.SetEnum(value); }

        /// <summary>
        /// 获取或设置是否计算文件的 SHA-512 哈希值。
        /// </summary>
        public bool SHA512 { get => this.IsEnum(); set => this.SetEnum(value); }
    }
}
