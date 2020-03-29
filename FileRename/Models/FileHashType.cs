namespace XstarS.FileRename.Models
{
    /// <summary>
    /// 表示文件哈希值的类型。
    /// </summary>
    public enum FileHashType
    {
        /// <summary>
        /// 表示无文件哈希值。
        /// </summary>
        Empty,
        /// <summary>
        /// 表示 MD5 文件哈希值。
        /// </summary>
        MD5,
        /// <summary>
        /// 表示 SHA-1 文件哈希值。
        /// </summary>
        SHA1,
        /// <summary>
        /// 表示 SHA-256 文件哈希值。
        /// </summary>
        SHA256,
        /// <summary>
        /// 表示 SHA-384 文件哈希值。
        /// </summary>
        SHA384,
        /// <summary>
        /// 表示 SHA-512 文件哈希值。
        /// </summary>
        SHA512
    }
}
