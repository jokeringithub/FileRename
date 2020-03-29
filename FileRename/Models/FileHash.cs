using System;
using System.IO;
using System.Security.Cryptography;
using mstring = System.Text.StringBuilder;

namespace XstarS.FileRename.Models
{
    /// <summary>
    /// 提供计算文件哈希值的方法。
    /// </summary>
    public class FileHash : IDisposable
    {
        /// <summary>
        /// 指示此实例的资源是否已经被释放。
        /// </summary>
        private volatile bool IsDisposed = false;

        /// <summary>
        /// 计算哈希值的文件流。
        /// </summary>
        private readonly Stream HashingFile;

        /// <summary>
        /// 使用文件和哈希值类型初始化 <see cref="FileHash"/> 的实例。
        /// </summary>
        /// <param name="filePath">要计算哈希值的文件路径。</param>
        /// <param name="hashType">要计算哈希值的类型。</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design", "CA1031:DoNotNatchGeneralExceptionTypes")]
        public FileHash(string filePath, FileHashType hashType)
        {
            this.FilePath = filePath;
            this.HashType = hashType;
            try { this.HashingFile = File.OpenRead(filePath); }
            catch (Exception) { this.HashingFile = Stream.Null; }
        }

        /// <summary>
        /// 获取要计算哈希值的文件路径。
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// 获取要计算哈希值的类型。
        /// </summary>
        public FileHashType HashType { get; }

        /// <summary>
        /// 获取文件哈希值的字节数组。
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public byte[] HashBytes { get; private set; }

        /// <summary>
        /// 获取文件哈希值的大写十六进制字符串。
        /// </summary>
        public string HashHexString
        {
            get
            {
                var bytes = this.HashBytes;
                if (bytes is null) { return null; }
                var hex = new mstring();
                foreach (var @byte in bytes)
                {
                    hex.Append(@byte.ToString("X2"));
                }
                return hex.ToString();
            }
        }

        /// <summary>
        /// 计算文件哈希值。
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design", "CA1031:DoNotNatchGeneralExceptionTypes")]
        public void Compute()
        {
            try
            {
                switch (this.HashType)
                {
                    case FileHashType.MD5:
                        this.HashBytes = MD5.Create().ComputeHash(HashingFile);
                        break;
                    case FileHashType.SHA1:
                        this.HashBytes = SHA1.Create().ComputeHash(HashingFile);
                        break;
                    case FileHashType.SHA256:
                        this.HashBytes = SHA256.Create().ComputeHash(HashingFile);
                        break;
                    case FileHashType.SHA384:
                        this.HashBytes = SHA384.Create().ComputeHash(HashingFile);
                        break;
                    case FileHashType.SHA512:
                        this.HashBytes = SHA512.Create().ComputeHash(HashingFile);
                        break;
                    default:
                        this.HashBytes = null;
                        break;
                }
            }
            catch (Exception)
            {
                this.HashBytes = null;
            }
        }

        /// <summary>
        /// 释放此实例占用的资源。
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// 释放当前实例占用的非托管资源，并根据指示释放托管资源。
        /// </summary>
        /// <param name="disposing">指示是否释放托管资源。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    this.HashingFile?.Dispose();
                }

                this.IsDisposed = true;
            }
        }
    }
}
