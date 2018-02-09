using System;
using System.IO;
using System.Security.Cryptography;

namespace FileHash
{
    public partial class FileHashParallel
    {
        /// <summary>
        /// 用于计算文件散列值的类
        /// </summary>
        private partial class FileHash : IDisposable
        {
            /// <summary>
            /// 实例化FileHash类
            /// </summary>
            /// <param name="fileStream">输入文件流</param>
            /// <param name="hashType">散列值类型</param>
            public FileHash(FileStream fileStream, HashType hashType)
            {
                this.fileStream = fileStream;
                this.hashType = hashType;
            }

            /// <summary>
            /// 计算散列值所用文件流
            /// </summary>
            private FileStream fileStream;
            /// <summary>
            /// 要计算的散列类型
            /// </summary>
            private HashType hashType;

            /// <summary>
            /// 散列值类型枚举类
            /// </summary>
            public enum HashType { CRC32, MD5, SHA1, SHA256, SHA384, SHA512 };

            /// <summary>
            /// 获取允许计算的散列值的类型数量
            /// </summary>
            public static int HashTypeCount { get => (Enum.GetNames(typeof(HashType))).Length; }

            /// <summary>
            /// 根据所选散列值类型计算文件散列值
            /// </summary>
            public void Compute()
            {
                try
                {
                    switch (hashType)
                    {
                        case HashType.CRC32:
                            fileHashBytes = CRC32.Create().ComputeHash(fileStream);
                            break;
                        case HashType.MD5:
                            fileHashBytes = MD5.Create().ComputeHash(fileStream);
                            break;
                        case HashType.SHA1:
                            fileHashBytes = SHA1.Create().ComputeHash(fileStream);
                            break;
                        case HashType.SHA256:
                            fileHashBytes = SHA256.Create().ComputeHash(fileStream);
                            break;
                        case HashType.SHA384:
                            fileHashBytes = SHA384.Create().ComputeHash(fileStream);
                            break;
                        case HashType.SHA512:
                            fileHashBytes = SHA512.Create().ComputeHash(fileStream);
                            break;
                        default:
                            fileHashBytes = null;
                            break;
                    }
                    fileStream.Dispose();
                }
                catch (Exception)
                {
                    fileHashBytes = null;
                    return;
                }
            }

            /// <summary>
            /// 释放此实例占用的资源
            /// </summary>
            public void Dispose()
            {
                if (fileStream != null)
                {
                    fileStream.Dispose();
                }
            }

            /// <summary>
            /// 实例销毁，释放此实例占用的资源
            /// </summary>
            ~FileHash()
            {
                Dispose();
            }

            /// <summary>
            /// 文件的散列值
            /// </summary>
            private byte[] fileHashBytes;
            /// <summary>
            /// 文件的散列值
            /// </summary>
            public byte[] FileHashBytes { get => fileHashBytes; set => fileHashBytes = value; }
            /// <summary>
            /// 目前计算的进度
            /// </summary>
            public double ComputeProgress
            {
                get
                {
                    if (fileStream != null)
                    {
                        try
                        {
                            return (double)fileStream.Position / fileStream.Length;
                        }
                        // 文件流已释放
                        catch (Exception)
                        {
                            return 1;
                        }
                    }
                    // 文件流未创建
                    else
                    {
                        return 0;
                    }
                }
            }
        }

        private partial class FileHash
        {
            /// <summary>
            /// 提供CRC32算法的实现。
            /// </summary>
            private class CRC32 : HashAlgorithm
            {
                /// <summary>
                /// 默认多项式
                /// </summary>
                public const uint DefaultPolynomial = 0xedb88320;
                /// <summary>
                /// 默认种子
                /// </summary>
                public const uint DefaultSeed = 0xffffffff;

                private uint hash;
                private uint seed;
                private uint[] table;
                private static uint[] defaultTable;

                /// <summary>
                /// 创建一个默认的实例实现CRC32算法。
                /// </summary>
                protected CRC32()
                {
                    this.table = InitializeTable(DefaultPolynomial);
                    this.seed = DefaultSeed;
                    Initialize();
                }

                /// <summary>
                /// 根据所给的多项式和种子创建实例实现CRC32算法。
                /// </summary>
                /// <param name="polynomial">所给的多项式</param>
                /// <param name="seed">所给的种子</param>
                protected CRC32(uint polynomial, uint seed)
                {
                    this.table = InitializeTable(polynomial);
                    this.seed = seed;
                    Initialize();
                }

                /// <summary>
                /// 创建一个默认的实例实现CRC32
                /// </summary>
                /// <returns>CRC32实例</returns>
                public new static CRC32 Create()
                {
                    return new CRC32();
                }

                /// <summary>
                /// 根据所给的多项式和种子创建实例实现CRC32
                /// </summary>
                /// <param name="polynomial">所给的多项式</param>
                /// <param name="seed">所给的种子</param>
                /// <returns>CRC32实例</returns>
                public static CRC32 Creat(uint polynomial, uint seed)
                {
                    return new CRC32(polynomial, seed);
                }

                /// <summary>
                /// 初始化CRC32实例
                /// </summary>
                public override void Initialize()
                {
                    hash = seed;
                }

                protected override void HashCore(byte[] buffer, int start, int length)
                {
                    hash = CalculateHash(table, hash, buffer, start, length);
                }

                protected override byte[] HashFinal()
                {
                    byte[] hashBuffer = UInt32ToBigEndianbytes(~hash);
                    this.HashValue = hashBuffer;
                    return hashBuffer;
                }

                /// <summary>
                /// 计算所给的字节数组的CRC32。
                /// </summary>
                /// <param name="buffer">要计算的字节数组</param>
                /// <returns>32位无符号整数CRC32</returns>
                public static uint Compute(byte[] buffer)
                {
                    return ~CalculateHash(InitializeTable(DefaultPolynomial), DefaultSeed, buffer, 0, buffer.Length);
                }

                /// <summary>
                /// 根据所给种子计算所给的字节数组的CRC32。
                /// </summary>
                /// <param name="seed">输入的种子</param>
                /// <param name="buffer">要计算的字节数组</param>
                /// <returns>32位无符号整数CRC32</returns>
                public static uint Compute(uint seed, byte[] buffer)
                {
                    return ~CalculateHash(InitializeTable(DefaultPolynomial), seed, buffer, 0, buffer.Length);
                }

                /// <summary>
                /// 根据所给种子和多项式计算所给的字节数组的CRC32。
                /// </summary>
                /// <param name="polynomial">输入的多项式</param>
                /// <param name="seed">输入的种子</param>
                /// <param name="buffer">要计算的字节数组</param>
                /// <returns>32位无符号整数CRC32</returns>
                public static uint Compute(uint polynomial, uint seed, byte[] buffer)
                {
                    return ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
                }

                private static uint[] InitializeTable(uint polynomial)
                {
                    if (polynomial == DefaultPolynomial && defaultTable != null)
                    {
                        return defaultTable;
                    }
                    uint[] createTable = new uint[256];
                    for (int i = 0; i < 256; i++)
                    {
                        uint entry = (uint)i;
                        for (int j = 0; j < 8; j++)
                        {
                            if ((entry & 1) == 1)
                                entry = (entry >> 1) ^ polynomial;
                            else
                                entry = entry >> 1;
                        }
                        createTable[i] = entry;
                    }
                    if (polynomial == DefaultPolynomial)
                    {
                        defaultTable = createTable;
                    }
                    return createTable;
                }

                private static uint CalculateHash(uint[] table, uint seed, byte[] buffer, int start, int size)
                {
                    uint crc = seed;
                    for (int i = start; i < size; i++)
                    {
                        unchecked
                        {
                            crc = (crc >> 8) ^ table[buffer[i] ^ crc & 0xff];
                        }
                    }
                    return crc;
                }

                private byte[] UInt32ToBigEndianbytes(uint x)
                {
                    return new byte[] { (byte)((x >> 24) & 0xff), (byte)((x >> 16) & 0xff), (byte)((x >> 8) & 0xff), (byte)(x & 0xff) };
                }
            }
        }
    }
}
