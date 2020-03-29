using XstarS.ComponentModel;

namespace XstarS.FileRename.Models
{
    /// <summary>
    /// 表示文件哈希值类型 <see cref="FileHashType"/> 的视图。
    /// </summary>
    public class FileHashTypeView : ObservableObject
    {
        /// <summary>
        /// 当前实例对应的 <see cref="FileHashType"/>。
        /// </summary>
        private FileHashType _Value;

        /// <summary>
        /// 初始化 <see cref="FileHashTypeView"/> 类的实例。
        /// </summary>
        public FileHashTypeView() { }

        /// <summary>
        /// 获取当前实例对应的 <see cref="FileHashType"/>。
        /// </summary>
        public FileHashType Value => this._Value;

        /// <summary>
        /// 获取或设置是否计算文件的 MD5 哈希值。
        /// </summary>
        public bool MD5
        {
            get => this.Value == FileHashType.MD5;
            set { if (value) { this.SetProperty(ref this._Value, FileHashType.MD5); } }
        }

        /// <summary>
        /// 获取或设置是否计算文件的 SHA-1 哈希值。
        /// </summary>
        public bool SHA1
        {
            get => this.Value == FileHashType.SHA1;
            set { if (value) { this.SetProperty(ref this._Value, FileHashType.SHA1); } }
        }

        /// <summary>
        /// 获取或设置是否计算文件的 SHA-256 哈希值。
        /// </summary>
        public bool SHA256
        {
            get => this.Value == FileHashType.SHA256;
            set { if (value) { this.SetProperty(ref this._Value, FileHashType.SHA256); } }
        }

        /// <summary>
        /// 获取或设置是否计算文件的 SHA-384 哈希值。
        /// </summary>
        public bool SHA384
        {
            get => this.Value == FileHashType.SHA384;
            set { if (value) { this.SetProperty(ref this._Value, FileHashType.SHA384); } }
        }

        /// <summary>
        /// 获取或设置是否计算文件的 SHA-512 哈希值。
        /// </summary>
        public bool SHA512
        {
            get => this.Value == FileHashType.SHA512;
            set { if (value) { this.SetProperty(ref this._Value, FileHashType.SHA512); } }
        }
    }
}
