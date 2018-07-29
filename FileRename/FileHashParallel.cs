using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FileHash
{
    /// <summary>
    /// 并行计算文件散列值类，依赖 <see cref="FileHashParallel.FileHash"/> 类。
    /// </summary>
    public partial class FileHashParallel : IDisposable
    {
        /// <summary>
        /// 获取允许计算的散列值的类型数量
        /// </summary>
        public static readonly int HashTypeCount = FileHash.HashTypeCount;

        /// <summary>
        /// 传入的文件路径。
        /// </summary>
        private readonly string filePath;
        /// <summary>
        /// 计算散列值的标志向量。
        /// </summary>
        private readonly bool[] fileHashEnables;
        /// <summary>
        /// 文件散列列表。
        /// </summary>
        private readonly List<byte[]> fileHashList;
        /// <summary>
        /// 后台线程，用于计算文件散列值。
        /// </summary>
        private readonly BackgroundWorker fileHashComputeBackgroundWorker;
        /// <summary>
        /// 计算散列类的数组。
        /// </summary>
        private FileHash[] fileHashes;
        /// <summary>
        /// 计算文件散列的文件流数组。
        /// </summary>
        private FileStream[] fileHashComputeFileStreams;
        /// <summary>
        /// 散列计算用线程。
        /// </summary>
        private Thread[] fileHashComputeThreads;

        /// <summary>
        /// 使用文件路径和计算散列值的标志向量初始化 <see cref="FileHashParallel"/> 的实例。
        /// </summary>
        /// <param name="filePath">文件路径。</param>
        /// <param name="fileHashEnables">计算散列值的标志向量。</param>
        /// <exception cref="ArgumentException"></exception>
        public FileHashParallel(string filePath, bool[] fileHashEnables)
        {
            // 输出标志向量长度错误时抛出异常。
            if (fileHashEnables.Length != HashTypeCount)
            {
                throw new ArgumentException();
            }

            this.filePath = filePath;
            this.fileHashEnables = fileHashEnables;
            this.fileHashList = new List<byte[]>();

            this.IsStarted = false;
            this.IsCompleted = false;
            this.IsCancelled = false;

            // 初始化后台线程。
            this.fileHashComputeBackgroundWorker = new BackgroundWorker() { WorkerSupportsCancellation = true };
            this.fileHashComputeBackgroundWorker.DoWork += this.FileHashComputeBackgroundWorker_DoWork;
        }

        /// <summary>
        /// <see cref="FileHashParallel"/> 实例的销毁，释放此实例占用的资源。
        /// </summary>
        ~FileHashParallel() => this.Dispose();

        /// <summary>
        /// 传入的文件路径。
        /// </summary>
        public string FilePath { get => this.filePath; }
        /// <summary>
        /// 指示计算是否已经开始。
        /// </summary>
        public bool IsStarted { get; private set; }
        /// <summary>
        /// 指示是否正在计算。
        /// </summary>
        public bool IsComputing { get => (this.IsStarted && !this.IsCompleted); }
        /// <summary>
        /// 指示计算是否完成。
        /// </summary>
        public bool IsCompleted { get; private set; }
        /// <summary>
        /// 指示是否被取消计算。
        /// </summary>
        public bool IsCancelled { get; private set; }
        /// <summary>
        /// 散列值计算进度。
        /// </summary>
        public double ComputeProgress
        {
            get
            {
                double computeProgress = 0;

                if (this.fileHashes != null)
                {
                    // 取得各个线程的计算进度
                    foreach (var fileHash in this.fileHashes)
                    {
                        if (fileHash != null)
                        {
                            computeProgress += fileHash.ComputeProgress;
                        }
                    }

                    // 获取总共启动的线程数量
                    int hashCount = 0;
                    foreach (bool fileHashEnable in this.fileHashEnables)
                    {
                        if (fileHashEnable)
                        {
                            hashCount++;
                        }
                    }

                    // 得到进度的平均值
                    if (hashCount != 0)
                    {
                        computeProgress /= hashCount;
                    }
                    else
                    {
                        computeProgress = 1;
                    }
                }
                else
                {
                    computeProgress = 0;
                }

                return computeProgress;
            }
        }

        /// <summary>
        /// 开始计算文件散列值。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileHashComputeBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            this.Compute();
        }

        /// <summary>
        /// 创建打开单一文件的文件流数组
        /// </summary>
        /// <exception cref="FileNotFoundException"></exception>
        private FileStream[] CreatFileStreams(string filePath, int count)
        {
            FileStream[] fileStreams = new FileStream[count];
            try
            {
                for (int i = 0; i < count; i++)
                {
                    fileStreams[i] = File.OpenRead(filePath);
                }
            }
            catch (Exception)
            {
                throw new FileNotFoundException(string.Empty, filePath);
            }
            return fileStreams;
        }

        /// <summary>
        /// 根据标志向量启动计算文件散列值的各个线程
        /// </summary>
        /// <exception cref="FileNotFoundException"></exception>
        private void Compute()
        {
            // 初始化计算散列值的各线程
            this.fileHashes = new FileHash[HashTypeCount];
            this.fileHashComputeThreads = new Thread[HashTypeCount];
            for (int i = 0; i < FileHashParallel.HashTypeCount; i++)
            {
                this.fileHashes[i] = new FileHash(this.fileHashComputeFileStreams[i], (FileHash.HashType)i);
                this.fileHashComputeThreads[i] = new Thread(this.fileHashes[i].Compute) { IsBackground = true };
            }

            // 根据标志位启动对应线程
            for (int i = 0; i < this.fileHashComputeThreads.Length; i++)
            {
                if (this.fileHashEnables[i] == true)
                {
                    try
                    {
                        this.fileHashComputeThreads[i].Start();
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            // 等待线程结束并取得计算结果
            while (this.fileHashComputeThreads.Any(fileHashComputeThread => fileHashComputeThread.IsAlive))
            {
                // 如果检测到取消操作，则直接退出，并触发计算完成事件，传递非正常完成参数
                if (this.fileHashComputeBackgroundWorker.CancellationPending)
                {
                    this.IsCompleted = true;
                    this.OnCompleted(this, new CompletedEventArgs(false));
                    return;
                }
                Thread.Sleep(10);
            }

            // 取出已经计算完成的结果，并触发计算完成事件，传递计算结果
            this.fileHashList.Clear();
            for (int i = 0; i < this.fileHashComputeThreads.Length; i++)
            {
                if (this.fileHashEnables[i] == true)
                {
                    this.fileHashList.Add(fileHashes[i].FileHashBytes);
                }
                else
                {
                    this.fileHashList.Add(null);
                }
            }
            IsCompleted = true;
            OnCompleted(this, new CompletedEventArgs(true, this));

            // 释放资源
            foreach (FileStream fileHashCalculateFileStream in fileHashComputeFileStreams)
            {
                fileHashCalculateFileStream.Dispose();
            }
        }

        /// <summary>
        /// 取消计算，退出各线程并释放资源
        /// </summary>
        private void ComputeCancelled()
        {
            // 令各计算散列的线程退出
            if (this.fileHashComputeThreads != null)
            {
                foreach (var hashCalcThread in this.fileHashComputeThreads)
                {
                    if ((hashCalcThread != null) && hashCalcThread.IsAlive)
                    {
                        hashCalcThread.Abort();
                    }
                }
            }

            // 令各文件流释放
            if (this.fileHashComputeFileStreams != null)
            {
                foreach (var hashCalculateFileStream in this.fileHashComputeFileStreams)
                {
                    hashCalculateFileStream?.Dispose();
                }
            }
        }

        /// <summary>
        /// 开始计算文件散列值，启动后台线程
        /// </summary>
        /// <exception cref="FileNotFoundException"></exception>
        public void StartAsync()
        {
            if (!this.fileHashComputeBackgroundWorker.IsBusy)
            {
                // 创建文件流并启动线程
                this.IsStarted = true;
                this.fileHashComputeFileStreams = this.CreatFileStreams(filePath, HashTypeCount);
                this.fileHashComputeBackgroundWorker.RunWorkerAsync();
            }
        }

        /// <summary>
        /// 取消计算文件散列值
        /// </summary>
        public void CancelAsync()
        {
            this.IsCompleted = true;
            this.IsCancelled = true;
            if (this.fileHashComputeBackgroundWorker.IsBusy)
            {
                this.fileHashComputeBackgroundWorker.CancelAsync();
            }
            this.ComputeCancelled();
        }

        /// <summary>
        /// 释放此实例占用的资源
        /// </summary>
        public void Dispose() => this.CancelAsync();

        /// <summary>
        /// 文件散列值到十六进制字符串
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public string[] ToHexStrings()
        {
            // 未完成则抛出异常
            if (!this.IsCompleted || this.IsCancelled)
            {
                throw new NotImplementedException();
            }

            // 初始化字符串数组并置为空，便于处理
            string[] fileHashHexStrings = new string[HashTypeCount];
            for (int i = 0; i < fileHashHexStrings.Length; i++)
            {
                fileHashHexStrings[i] = string.Empty;
            }

            // 取出散列值
            for (int i = 0; i < FileHashParallel.HashTypeCount; i++)
            {
                if (this.fileHashEnables[i])
                {
                    string hexHashString = string.Empty;
                    foreach (byte fileHashByte in this.fileHashList[i])
                    {
                        hexHashString += fileHashByte.ToString("x2");
                    }
                    fileHashHexStrings[i] = hexHashString;
                }
            }

            return fileHashHexStrings;
        }

        /// <summary>
        /// 文件散列值到小写十六进制字符串
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public string[] ToLowerHexStrings()
        {
            string[] fileHashHexStrings = this.ToHexStrings();
            for (int i = 0; i < fileHashHexStrings.Length; i++)
            {
                fileHashHexStrings[i] = fileHashHexStrings[i].ToLower();
            }
            return fileHashHexStrings;
        }

        /// <summary>
        /// 文件散列值到大写十六进制字符串
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public string[] ToUpperHexStrings()
        {
            string[] fileHashHexStrings = this.ToHexStrings();
            for (int i = 0; i < fileHashHexStrings.Length; i++)
            {
                fileHashHexStrings[i] = fileHashHexStrings[i].ToUpper();
            }
            return fileHashHexStrings;
        }

        /// <summary>
        /// 文件散列值到Base64字符串
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public string[] ToBase64Strings()
        {
            // 未完成则抛出异常
            if (!this.IsCompleted || this.IsCancelled)
            {
                throw new NotImplementedException();
            }

            // 初始化字符串数组并置为空，便于处理
            string[] fileHashBase64Strings = new string[FileHashParallel.HashTypeCount];
            for (int i = 0; i < fileHashBase64Strings.Length; i++)
            {
                fileHashBase64Strings[i] = string.Empty;
            }

            // 取出散列值
            for (int i = 0; i < FileHashParallel.HashTypeCount; i++)
            {
                if (this.fileHashEnables[i])
                {
                    fileHashBase64Strings[i] = Convert.ToBase64String(this.fileHashList[i]);
                }
            }

            return fileHashBase64Strings;
        }
    }

    public partial class FileHashParallel
    {
        /// <summary>
        /// 计算完成事件委托处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void CompletedEventHandler(object sender, CompletedEventArgs e);

        /// <summary>
        /// 计算完成事件
        /// </summary>
        public event CompletedEventHandler Completed;

        /// <summary>
        /// 触发计算完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnCompleted(object sender, CompletedEventArgs e) =>
            this.Completed?.Invoke(sender, e);

        /// <summary>
        /// 计算完成事件参数传递
        /// </summary>
        public class CompletedEventArgs : EventArgs
        {
            /// <summary>
            /// 以是否正常完成标志作为参数实例化此类
            /// </summary>
            /// <param name="isNormallyCompleted"></param>
            public CompletedEventArgs(bool isNormallyCompleted)
            {
                this.IsNormallyCompleted = isNormallyCompleted;
            }

            /// <summary>
            /// 以是否正常完成标志和计算结果作为参数实例化此类
            /// </summary>
            /// <param name="isNormallyCompleted"></param>
            /// <param name="result"></param>
            public CompletedEventArgs(bool isNormallyCompleted, FileHashParallel result)
            {
                this.IsNormallyCompleted = isNormallyCompleted;
                this.Result = result;
            }

            /// <summary>
            /// 指示是否正常完成的标志位
            /// </summary>
            public bool IsNormallyCompleted { get; }
            /// <summary>
            /// 计算得到的散列值列表
            /// </summary>
            public FileHashParallel Result { get; }
        }
    }
}
