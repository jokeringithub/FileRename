using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace FileHash
{
    /// <summary>
    /// 用于计算文件散列值的类。
    /// </summary>
    public partial class FileHashCompute : IDisposable
    {
        /// <summary>
        /// 获取允许计算的散列值的类型数量。
        /// </summary>
        public static readonly int HashTypeCount = Enum.GetNames(typeof(HashType)).Length;

        /// <summary>
        /// 计算散列值所用文件流。
        /// </summary>
        private readonly FileStream fileStream;
        /// <summary>
        /// 要计算的散列类型。
        /// </summary>
        private readonly HashType hashType;
        /// <summary>
        /// 指示此实例的资源是否已经被释放。
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// 初始化 <see cref="FileHashCompute"/> 的实例。
        /// </summary>
        /// <param name="fileStream">输入文件流。</param>
        /// <param name="hashType">散列值类型。</param>
        public FileHashCompute(FileStream fileStream, HashType hashType)
        {
            this.fileStream = fileStream;
            this.hashType = hashType;
            this.isDisposed = false;
        }

        /// <summary>
        /// <see cref="FileHashParallel"/> 实例的销毁，释放此实例占用的资源。
        /// </summary>
        ~FileHashCompute() => this.Dispose();

        /// <summary>
        /// 文件的散列值。
        /// </summary>
        public byte[] FileHashBytes { get; private set; }
        /// <summary>
        /// 目前计算的进度。
        /// </summary>
        public double ComputeProgress
        {
            get
            {
                if (this.fileStream != null)
                {
                    try
                    {
                        return (double)this.fileStream.Position / this.fileStream.Length;
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

        /// <summary>
        /// 根据所选散列值类型计算文件散列值
        /// </summary>
        public void Compute()
        {
            try
            {
                switch (this.hashType)
                {
                    case HashType.CRC32:
                        this.FileHashBytes = CRC32.Create().ComputeHash(fileStream);
                        break;
                    case HashType.MD5:
                        this.FileHashBytes = MD5.Create().ComputeHash(fileStream);
                        break;
                    case HashType.SHA1:
                        this.FileHashBytes = SHA1.Create().ComputeHash(fileStream);
                        break;
                    case HashType.SHA256:
                        this.FileHashBytes = SHA256.Create().ComputeHash(fileStream);
                        break;
                    case HashType.SHA384:
                        this.FileHashBytes = SHA384.Create().ComputeHash(fileStream);
                        break;
                    case HashType.SHA512:
                        this.FileHashBytes = SHA512.Create().ComputeHash(fileStream);
                        break;
                    default:
                        this.FileHashBytes = null;
                        break;
                }
                this.Dispose();
            }
            catch (Exception)
            {
                this.FileHashBytes = null;
                return;
            }
        }

        /// <summary>
        /// 释放此实例占用的资源
        /// </summary>
        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.fileStream?.Dispose();
                this.isDisposed = true;
            }
        }
    }

    public partial class FileHashCompute
    {
        /// <summary>
        /// 提供CRC32算法的实现。
        /// </summary>
        protected class CRC32 : HashAlgorithm
        {
            /// <summary>
            /// 默认多项式。
            /// </summary>
            public const uint DefaultPolynomial = 0xedb88320;
            /// <summary>
            /// 默认种子。
            /// </summary>
            public const uint DefaultSeed = 0xffffffff;
            /// <summary>
            /// 默认哈希表。
            /// </summary>
            private static uint[] defaultTable;

            /// <summary>
            /// 当前 CRC32 值。
            /// </summary>
            private uint crc32;
            /// <summary>
            /// 种子。
            /// </summary>
            private readonly uint seed;
            /// <summary>
            /// 哈希表。
            /// </summary>
            private readonly uint[] table;

            /// <summary>
            /// 创建一个默认的实例实现CRC32算法。
            /// </summary>
            protected CRC32()
            {
                this.table = InitializeTable(DefaultPolynomial);
                this.seed = DefaultSeed;
                this.Initialize();
            }

            /// <summary>
            /// 根据所给的多项式和种子创建实例实现 CRC32 算法。
            /// </summary>
            /// <param name="polynomial">多项式。</param>
            /// <param name="seed">种子。</param>
            protected CRC32(uint polynomial, uint seed)
            {
                this.table = InitializeTable(polynomial);
                this.seed = seed;
                this.Initialize();
            }

            /// <summary>
            /// 创建一个默认的实例实现 CRC32。
            /// </summary>
            /// <returns>CRC32 实例。</returns>
            new public static CRC32 Create()
            {
                return new CRC32();
            }

            /// <summary>
            /// 根据所给的多项式和种子创建实例实现 CRC32。
            /// </summary>
            /// <param name="polynomial">所给的多项式。</param>
            /// <param name="seed">所给的种子。</param>
            /// <returns>CRC32 实例。</returns>
            public static CRC32 Create(uint polynomial, uint seed)
            {
                return new CRC32(polynomial, seed);
            }

            /// <summary>
            /// 初始化 CRC32 实例。
            /// </summary>
            public override void Initialize()
            {
                this.crc32 = this.seed;
            }

            /// <summary>
            /// 将写入对象的数据路由到哈希算法以计算哈希值。
            /// </summary>
            /// <param name="array">要计算其哈希代码的输入。</param>
            /// <param name="ibStart">字节数组中的偏移量，从该位置开始使用数据。</param>
            /// <param name="cbSize">字节数组中用作数据的字节数。</param>
            protected override void HashCore(byte[] array, int ibStart, int cbSize)
            {
                this.crc32 = CRC32.CalculateHash(this.table, this.crc32, array, ibStart, cbSize);
            }

            /// <summary>
            /// 在加密流对象处理完最后的数据后完成哈希计算。
            /// </summary>
            /// <returns>计算所得的哈希代码。</returns>
            protected override byte[] HashFinal()
            {
                byte[] hashValue = this.UInt32ToBigEndianbytes(~crc32);
                this.HashValue = hashValue;
                return hashValue;
            }

            /// <summary>
            /// 计算所给的字节数组的 CRC32。
            /// </summary>
            /// <param name="array">要计算的字节数组。</param>
            /// <returns>32 位无符号整数 CRC32。</returns>
            public static uint Compute(byte[] array) =>
                ~CRC32.CalculateHash(
                    CRC32.InitializeTable(CRC32.DefaultPolynomial), CRC32.DefaultSeed, array, 0, array.Length);

            /// <summary>
            /// 根据所给种子计算所给的字节数组的 CRC32。
            /// </summary>
            /// <param name="seed">种子。</param>
            /// <param name="array">要计算的字节数组。</param>
            /// <returns>32 位无符号整数 CRC32。</returns>
            public static uint Compute(uint seed, byte[] array) =>
                ~CRC32.CalculateHash(
                    CRC32.InitializeTable(CRC32.DefaultPolynomial), seed, array, 0, array.Length);

            /// <summary>
            /// 根据所给种子和多项式计算所给的字节数组的 CRC32。
            /// </summary>
            /// <param name="polynomial">输入的多项式。</param>
            /// <param name="seed">输入的种子。</param>
            /// <param name="array">要计算的字节数组。</param>
            /// <returns>32 位无符号整数 CRC32。</returns>
            public static uint Compute(uint polynomial, uint seed, byte[] array) =>
                ~CRC32.CalculateHash(
                    CRC32.InitializeTable(polynomial), seed, array, 0, array.Length);

            /// <summary>
            /// 根据指定多项式初始化哈希表。
            /// </summary>
            /// <param name="polynomial">多项式。</param>
            /// <returns>哈希表。</returns>
            private static uint[] InitializeTable(uint polynomial)
            {
                if (polynomial == CRC32.DefaultPolynomial && CRC32.defaultTable != null)
                {
                    return CRC32.defaultTable;
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
                    CRC32.defaultTable = createTable;
                }
                return createTable;
            }

            /// <summary>
            /// 根据所给哈希表和种子计算所给的字节数组的 CRC32。
            /// </summary>
            /// <param name="table">哈希表。</param>
            /// <param name="seed">种子。</param>
            /// <param name="array">要计算其哈希代码的输入。</param>
            /// <param name="ibStart">字节数组中的偏移量，从该位置开始使用数据。</param>
            /// <param name="cbSize">字节数组中用作数据的字节数。</param>
            /// <returns>32 位无符号整数 CRC32。</returns>
            private static uint CalculateHash(uint[] table, uint seed, byte[] array, int ibStart, int cbSize)
            {
                uint crc32 = seed;
                for (int i = ibStart; i < cbSize; i++)
                {
                    unchecked
                    {
                        crc32 = (crc32 >> 8) ^ table[array[i] ^ crc32 & 0xff];
                    }
                }
                return crc32;
            }

            /// <summary>
            /// 将 32 位无符号整数转换为大端序 8 位无符号整数数组。
            /// </summary>
            /// <param name="value">要转换的 32 为无符号整数。</param>
            /// <returns>与 <paramref name="value"/> 等效的大端序 8 位无符号整数数组。</returns>
            private byte[] UInt32ToBigEndianbytes(uint value) =>
                new byte[] {
                        (byte)((value >> 24) & 0xff),
                        (byte)((value >> 16) & 0xff),
                        (byte)((value >> 8) & 0xff),
                        (byte)(value & 0xff) };
        }
    }
}
