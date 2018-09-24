using System;

namespace FileHash
{
    /// <summary>
    /// 散列值类型枚举类。
    /// </summary>
    public enum HashType
    {
        /// <summary>
        /// CRC32。
        /// </summary>
        CRC32,
        /// <summary>
        /// MD5。
        /// </summary>
        MD5,
        /// <summary>
        /// SHA-1。
        /// </summary>
        SHA1,
        /// <summary>
        /// SHA-256。
        /// </summary>
        SHA256,
        /// <summary>
        /// SHA-384。
        /// </summary>
        SHA384,
        /// <summary>
        /// SHA-512。
        /// </summary>
        SHA512,
    }
}
