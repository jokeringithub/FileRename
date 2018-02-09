using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileRenamer
{
    /// <summary>
    /// 计算文件哈希值的类
    /// </summary>
    public class FileHash : IDisposable
    {
        /// <summary>
        /// 使用文件路径和指示变量实例化FileHash类
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="hashCodeSelected">指示计算哪些哈希值，长度为4，从0到3分别为MD5、SHA-1、SHA-256、SHA-512</param>
        /// <exception cref="FileNotFoundException"></exception>
        public FileHash(string filePath, bool[] hashCodeSelected)
        {
            // 长度异常抛出错误
            if (hashCodeSelected.Length != 4)
            {
                throw new ArgumentException();
            }

            // 计算哈希值
            CalculateHash(filePath, hashCodeSelected);
        }

        /// <summary>
        /// FileHash类实例的销毁
        /// </summary>
        ~FileHash()
        {
            Dispose();
        }

        /// <summary>
        /// 释放使用的资源
        /// </summary>
        public void Dispose()
        {
            if (md5Thread != null)
            { md5Thread.Abort(); }
            if (sha1Thread != null)
            { sha1Thread.Abort(); }
            if (sha256Thread != null)
            { sha256Thread.Abort(); }
            if (sha512Thread != null)
            { sha512Thread.Abort(); }
            // 回收垃圾
            GC.Collect();
        }

        // 将各线程置为字段，保证类的实例销毁时线程能够正常退出
        private Thread md5Thread;
        private Thread sha1Thread;
        private Thread sha256Thread;
        private Thread sha512Thread;

        /// <summary>
        /// 文件MD5值
        /// </summary>
        private byte[] fileHashMD5;
        /// <summary>
        /// 文件SHA1值
        /// </summary>
        private byte[] fileHashSHA1;
        /// <summary>
        /// 文件SHA256值
        /// </summary>
        private byte[] fileHashSHA256;
        /// <summary>
        /// 文件SHA512值
        /// </summary>
        private byte[] fileHashSHA512;

        /// <summary>
        /// 文件MD5值
        /// </summary>
        public byte[] FileHashMD5 { get => fileHashMD5; }
        /// <summary>
        /// 文件SHA1值
        /// </summary>
        public byte[] FileHashSHA1 { get => fileHashSHA1; }
        /// <summary>
        /// 文件SHA256值
        /// </summary>
        public byte[] FileHashSHA256 { get => fileHashSHA256; }
        /// <summary>
        /// 文件SHA512值
        /// </summary>
        public byte[] FileHashSHA512 { get => fileHashSHA512; }

        /// <summary>
        /// 文件MD5值的十六进制表示法
        /// </summary>
        public string FileHashMD5HexString
        {
            get
            {
                string hexString = string.Empty;
                try
                {
                    foreach (byte fileHashMD5Byte in fileHashMD5)
                    {
                        hexString += fileHashMD5Byte.ToString("x2").ToUpper();
                    }
                }
                catch (Exception)
                {
                    hexString = null;
                }
                return hexString;
            }
        }
        /// <summary>
        /// 文件SHA1值的十六进制表示法
        /// </summary>
        public string FileHashSHA1HexString
        {
            get
            {
                string hexString = string.Empty;
                try
                {
                    foreach (byte fileHashSHA1Byte in fileHashSHA1)
                    {
                        hexString += fileHashSHA1Byte.ToString("x2").ToUpper();
                    }
                }
                catch (Exception)
                {
                    hexString = null;
                }
                return hexString;
            }
        }
        /// <summary>
        /// 文件SHA256值的十六进制表示法
        /// </summary>
        public string FileHashSHA256HexString
        {
            get
            {
                string hexString = string.Empty;
                try
                {
                    foreach (byte fileHashSHA256Byte in fileHashSHA256)
                    {
                        hexString += fileHashSHA256Byte.ToString("x2").ToUpper();
                    }
                }
                catch (Exception)
                {
                    hexString = null;
                }
                return hexString;
            }
        }
        /// <summary>
        /// 文件SHA512值的十六进制表示法
        /// </summary>
        public string FileHashSHA512HexString
        {
            get
            {
                string hexString = string.Empty;
                try
                {
                    foreach (byte fileHashSHA512Byte in fileHashSHA512)
                    {
                        hexString += fileHashSHA512Byte.ToString("x2").ToUpper();
                    }
                }
                catch (Exception)
                {
                    hexString = null;
                }
                return hexString;
            }
        }

        /// <summary>
        /// 文件MD5值的Base64表示法
        /// </summary>
        public string FileHashMD5Base64String
        {
            get
            {
                string base64String = string.Empty;
                try
                {
                    base64String = Convert.ToBase64String(fileHashMD5);
                }
                catch (Exception)
                {
                    base64String = null;
                }
                return base64String;
            }
        }
        /// <summary>
        /// 文件SHA1值的Base64表示法
        /// </summary>
        public string FileHashSHA1Base64String
        {
            get
            {
                string base64String = string.Empty;
                try
                {
                    base64String = Convert.ToBase64String(fileHashSHA1);
                }
                catch (Exception)
                {
                    base64String = null;
                }
                return base64String;
            }
        }
        /// <summary>
        /// 文件SHA256值的Base64表示法
        /// </summary>
        public string FileHashSHA256Base64String
        {
            get
            {
                string base64String = string.Empty;
                try
                {
                    base64String = Convert.ToBase64String(fileHashSHA256);
                }
                catch (Exception)
                {
                    base64String = null;
                }
                return base64String;
            }
        }
        /// <summary>
        /// 文件SHA512值的Base64表示法
        /// </summary>
        public string FileHashSHA512Base64String
        {
            get
            {
                string base64String = string.Empty;
                try
                {
                    base64String = Convert.ToBase64String(fileHashSHA512);
                }
                catch (Exception)
                {
                    base64String = null;
                }
                return base64String;
            }
        }

        /// <summary>
        /// 计算文件哈希值的方法
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="hashCodeSelected">指示计算哪些哈希值，从0到3分别为MD5、SHA-1、SHA-256、SHA-512</param>
        /// <exception cref="FileNotFoundException"></exception>
        private void CalculateHash(string filePath, bool[] hashCodeSelected)
        {
            // 尝试打开文件流
            FileStream fileStream;
            try
            {
                fileStream = File.OpenRead(filePath);
            }
            catch (Exception)
            {
                throw new FileLoadException();
            }
            fileStream.Close();

            // 初始化计算哈希值的各线程
            // MD5
            FileStream md5FileStream = File.OpenRead(filePath);
            ComputeMD5 computeMD5 = new ComputeMD5(md5FileStream);
            md5Thread = new Thread(computeMD5.Compute) { IsBackground = true };
            // SHA1
            FileStream sha1FileStream = File.OpenRead(filePath);
            ComputeSHA1 computeSHA1 = new ComputeSHA1(sha1FileStream);
            sha1Thread = new Thread(computeSHA1.Compute) { IsBackground = true };
            // SHA256
            FileStream sha256FileStream = File.OpenRead(filePath);
            ComputeSHA256 computeSHA256 = new ComputeSHA256(sha256FileStream);
            sha256Thread = new Thread(computeSHA256.Compute) { IsBackground = true };
            // SHA512
            FileStream sha512FileStream = File.OpenRead(filePath);
            ComputeSHA512 computeSHA512 = new ComputeSHA512(sha512FileStream);
            sha512Thread = new Thread(computeSHA512.Compute) { IsBackground = true };

            // 根据标志位启动对应线程
            // MD5
            if (hashCodeSelected[0] == true)
            { md5Thread.Start(); }
            // SHA1
            if (hashCodeSelected[1] == true)
            { sha1Thread.Start(); }
            // SHA256
            if (hashCodeSelected[2] == true)
            { sha256Thread.Start(); }
            // SHA512
            if (hashCodeSelected[3] == true)
            { sha512Thread.Start(); }

            // 等待线程结束并取得计算结果
            while (md5Thread.IsAlive || sha1Thread.IsAlive
                   || sha256Thread.IsAlive || sha512Thread.IsAlive)
            { Thread.Sleep(10); }
            // MD5
            if (hashCodeSelected[0] == true)
            { fileHashMD5 = computeMD5.FileHashMD5; }
            // SHA1
            if (hashCodeSelected[1] == true)
            { fileHashSHA1 = computeSHA1.FileHashSHA1; }
            // SHA256
            if (hashCodeSelected[2] == true)
            { fileHashSHA256 = computeSHA256.FileHashSHA256; }
            // SHA512
            if (hashCodeSelected[3] == true)
            { fileHashSHA512 = computeSHA512.FileHashSHA512; }

            // 释放资源
            md5FileStream.Close();
            sha1FileStream.Close();
            sha256FileStream.Close();
            sha512FileStream.Close();
            GC.Collect();
        }

        #region 多线程相关类

        /// <summary>
        /// 计算文件MD5的类
        /// </summary>
        private class ComputeMD5
        {
            /// <summary>
            /// 初始化ComputeMD5类
            /// </summary>
            /// <param name="stream"></param>
            public ComputeMD5(FileStream stream)
            {
                fileStream = stream;
            }

            /// <summary>
            /// 计算文件的MD5
            /// </summary>
            public void Compute()
            {
                MD5 md5 = MD5.Create();
                fileHashMD5 = md5.ComputeHash(fileStream);
                fileStream.Close();
            }

            /// <summary>
            /// 文件流
            /// </summary>
            FileStream fileStream;

            /// <summary>
            /// 文件的MD5
            /// </summary>
            private byte[] fileHashMD5;

            /// <summary>
            /// 文件的MD5
            /// </summary>
            public byte[] FileHashMD5 { get => fileHashMD5; set => fileHashMD5 = value; }
        }

        /// <summary>
        /// 计算文件SHA1的类
        /// </summary>
        private class ComputeSHA1
        {
            /// <summary>
            /// 初始化ComputeSHA1类
            /// </summary>
            /// <param name="stream"></param>
            public ComputeSHA1(FileStream stream)
            {
                fileStream = stream;
            }

            /// <summary>
            /// 计算文件的SHA1
            /// </summary>
            public void Compute()
            {
                SHA1 sha1 = SHA1.Create();
                fileHashSHA1 = sha1.ComputeHash(fileStream);
                fileStream.Close();
            }

            /// <summary>
            /// 文件流
            /// </summary>
            FileStream fileStream;

            /// <summary>
            /// 文件的SHA1
            /// </summary>
            private byte[] fileHashSHA1;

            /// <summary>
            /// 文件的SHA1
            /// </summary>
            public byte[] FileHashSHA1 { get => fileHashSHA1; set => fileHashSHA1 = value; }
        }

        /// <summary>
        /// 计算文件SHA256的类
        /// </summary>
        private class ComputeSHA256
        {
            /// <summary>
            /// 初始化ComputeSHA256类
            /// </summary>
            /// <param name="stream"></param>
            public ComputeSHA256(FileStream stream)
            {
                fileStream = stream;
            }

            /// <summary>
            /// 计算文件的SHA256
            /// </summary>
            public void Compute()
            {
                SHA256 sha256 = SHA256.Create();
                fileHashSHA256 = sha256.ComputeHash(fileStream);
                fileStream.Close();
            }

            /// <summary>
            /// 文件流
            /// </summary>
            FileStream fileStream;

            /// <summary>
            /// 文件的SHA256
            /// </summary>
            private byte[] fileHashSHA256;

            /// <summary>
            /// 文件的SHA256
            /// </summary>
            public byte[] FileHashSHA256 { get => fileHashSHA256; set => fileHashSHA256 = value; }
        }

        /// <summary>
        /// 计算文件SHA512的类
        /// </summary>
        private class ComputeSHA512
        {
            /// <summary>
            /// 初始化ComputeSHA512类
            /// </summary>
            /// <param name="stream"></param>
            public ComputeSHA512(FileStream stream)
            {
                fileStream = stream;
            }

            /// <summary>
            /// 计算文件的SHA512
            /// </summary>
            public void Compute()
            {
                SHA512 sha512 = SHA512.Create();
                fileHashSHA512 = sha512.ComputeHash(fileStream);
                fileStream.Close();
            }

            /// <summary>
            /// 文件流
            /// </summary>
            FileStream fileStream;

            /// <summary>
            /// 文件的SHA512
            /// </summary>
            private byte[] fileHashSHA512;

            /// <summary>
            /// 文件的SHA512
            /// </summary>
            public byte[] FileHashSHA512 { get => fileHashSHA512; set => fileHashSHA512 = value; }
        }

        #endregion 多线程相关类

        /// <summary>
        /// 获取已计算的文件哈希值的十六进制字符表示形式
        /// </summary>
        /// <returns></returns>
        public string ToHexString()
        {
            string hexString = string.Empty;
            if (fileHashMD5 != null)
            {
                hexString += FileHashMD5HexString;
            }
            if (fileHashSHA1 != null)
            {
                hexString += FileHashSHA1HexString;
            }
            if (fileHashSHA256 != null)
            {
                hexString += FileHashSHA256HexString;
            }
            if (fileHashSHA512 != null)
            {
                hexString += FileHashSHA512HexString;
            }
            return hexString;
        }
    }
}
