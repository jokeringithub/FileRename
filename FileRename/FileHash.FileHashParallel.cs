using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FileHash
{
    /// <summary>
    /// 并行计算文件散列值类，依赖FileHashParallel.FileHash类
    /// </summary>
    public partial class FileHashParallel : IDisposable
    {
        /// <summary>
        /// 使用文件路径和计算散列值的标志向量实例化此类
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileHashEnables">计算散列值的标志向量</param>
        /// <exception cref="ArgumentException"></exception>
        public FileHashParallel(string filePath, bool[] fileHashEnables)
        {
            // 输出标志向量长度错误时抛出异常
            if (fileHashEnables.Length != HashTypeCount)
            {
                throw new ArgumentException();
            }

            this.filePath = filePath;
            this.fileHashEnables = fileHashEnables;

            isStarted = false;
            isCompleted = false;
            isCancelled = false;

            InitializeBackgroundWorker();
        }

        /// <summary>
        /// 传入的文件路径
        /// </summary>
        private string filePath;
        /// <summary>
        /// 计算散列值的标志向量
        /// </summary>
        private bool[] fileHashEnables;

        /// <summary>
        /// 后台线程，用于计算文件散列值
        /// </summary>
        private BackgroundWorker fileHashComputeBackgroundWorker;
        /// <summary>
        /// 计算散列类的数组
        /// </summary>
        private FileHash[] fileHashes;
        /// <summary>
        /// 计算文件散列的文件流数组
        /// </summary>
        private FileStream[] fileHashComputeFileStreams;
        /// <summary>
        /// 散列计算用线程
        /// </summary>
        private Thread[] fileHashComputeThreads;

        /// <summary>
        /// 初始化后台线程
        /// </summary>
        private void InitializeBackgroundWorker()
        {
            fileHashComputeBackgroundWorker = new BackgroundWorker() { WorkerSupportsCancellation = true };
            fileHashComputeBackgroundWorker.DoWork += FileHashComputeBackgroundWorker_DoWork;
        }

        /// <summary>
        /// 开始计算文件散列值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileHashComputeBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Compute();
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
            fileHashes = new FileHash[HashTypeCount];
            fileHashComputeThreads = new Thread[HashTypeCount];
            for (int i = 0; i < HashTypeCount; i++)
            {
                fileHashes[i] = new FileHash(fileHashComputeFileStreams[i], (FileHash.HashType)i);
                fileHashComputeThreads[i] = new Thread(fileHashes[i].Compute) { IsBackground = true };
            }

            // 根据标志位启动对应线程
            for (int i = 0; i < fileHashComputeThreads.Length; i++)
            {
                if (fileHashEnables[i] == true)
                {
                    try
                    {
                        fileHashComputeThreads[i].Start();
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            // 等待线程结束并取得计算结果
            while (fileHashComputeThreads.Any(fileHashComputeThread => fileHashComputeThread.IsAlive))
            {
                // 如果检测到取消操作，则直接退出，并触发计算完成事件，传递非正常完成参数
                if (fileHashComputeBackgroundWorker.CancellationPending)
                {
                    isCompleted = true;
                    OnCompleted(this, new CompletedEventArgs(false));
                    return;
                }
                Thread.Sleep(10);
            }

            // 取出已经计算完成的结果，并触发计算完成事件，传递计算结果
            fileHashList = new List<byte[]>();
            for (int i = 0; i < fileHashComputeThreads.Length; i++)
            {
                if (fileHashEnables[i] == true)
                {
                    fileHashList.Add(fileHashes[i].FileHashBytes);
                }
                else
                {
                    fileHashList.Add(null);
                }
            }
            isCompleted = true;
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
            if (fileHashComputeThreads != null)
            {
                foreach (Thread hashCalcThread in fileHashComputeThreads)
                {
                    if ((hashCalcThread != null) && (hashCalcThread.IsAlive))
                    {
                        hashCalcThread.Abort();
                    }
                }
            }

            // 令各文件流释放
            if (fileHashComputeFileStreams != null)
            {
                foreach (FileStream hashCalculateFileStream in fileHashComputeFileStreams)
                {
                    if (hashCalculateFileStream != null)
                    {
                        hashCalculateFileStream.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// 开始计算文件散列值，启动后台线程
        /// </summary>
        /// <exception cref="FileNotFoundException"></exception>
        public void StartAsync()
        {
            if (!fileHashComputeBackgroundWorker.IsBusy)
            {
                // 创建文件流并启动线程
                isStarted = true;
                fileHashComputeFileStreams = CreatFileStreams(filePath, HashTypeCount);
                fileHashComputeBackgroundWorker.RunWorkerAsync();
            }
        }

        /// <summary>
        /// 取消计算文件散列值
        /// </summary>
        public void CancelAsync()
        {
            isCompleted = true;
            isCancelled = true;
            if (fileHashComputeBackgroundWorker.IsBusy)
            {
                fileHashComputeBackgroundWorker.CancelAsync();
            }
            ComputeCancelled();
        }

        /// <summary>
        /// 文件散列值到十六进制字符串
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public string[] ToHexStrings()
        {
            // 未完成则抛出异常
            if (!isCompleted || isCancelled)
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
            for (int i = 0; i < HashTypeCount; i++)
            {
                if (fileHashEnables[i])
                {
                    string hexHashString = string.Empty;
                    foreach (byte fileHashByte in fileHashList[i])
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
            string[] fileHashHexStrings = ToHexStrings();
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
            string[] fileHashHexStrings = ToHexStrings();
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
            if (!isCompleted || isCancelled)
            {
                throw new NotImplementedException();
            }

            // 初始化字符串数组并置为空，便于处理
            string[] fileHashBase64Strings = new string[HashTypeCount];
            for (int i = 0; i < fileHashBase64Strings.Length; i++)
            {
                fileHashBase64Strings[i] = string.Empty;
            }

            // 取出散列值
            for (int i = 0; i < HashTypeCount; i++)
            {
                if (fileHashEnables[i])
                {
                    fileHashBase64Strings[i] = Convert.ToBase64String(fileHashList[i]);
                }
            }

            return fileHashBase64Strings;
        }

        /// <summary>
        /// 释放此实例占用的资源
        /// </summary>
        public void Dispose()
        {
            CancelAsync();
        }

        /// <summary>
        /// 实例销毁，释放此实例占用的资源
        /// </summary>
        ~FileHashParallel()
        {
            Dispose();
        }

        /// <summary>
        /// 传入的文件路径
        /// </summary>
        public string FilePath { get => filePath; }
        /// <summary>
        /// 获取允许计算的散列值的类型数量
        /// </summary>
        public static int HashTypeCount { get => FileHash.HashTypeCount; }
        /// <summary>
        /// 文件散列列表
        /// </summary>
        private List<byte[]> fileHashList;
        /// <summary>
        /// 指示计算是否已经开始
        /// </summary>
        private bool isStarted;
        /// <summary>
        /// 指示计算是否已经开始
        /// </summary>
        public bool IsStarted { get => isStarted; }
        /// <summary>
        /// 指示是否正在计算
        /// </summary>
        public bool IsComputing { get => (isStarted && !IsCompleted); }
        /// <summary>
        /// 指示计算是否完成
        /// </summary>
        private bool isCompleted;
        /// <summary>
        /// 指示计算是否完成
        /// </summary>
        public bool IsCompleted { get => isCompleted; }
        /// <summary>
        /// 指示是否被取消计算
        /// </summary>
        private bool isCancelled;
        /// <summary>
        /// 指示是否被取消计算
        /// </summary>
        public bool IsCancelled { get => isCancelled; }
        /// <summary>
        /// 散列值计算进度
        /// </summary>
        public double ComputeProgress
        {
            get
            {
                double computeProgress = 0;

                if (fileHashes != null)
                {
                    // 取得各个线程的计算进度
                    foreach (FileHash fileHash in fileHashes)
                    {
                        if (fileHash != null)
                        {
                            computeProgress += fileHash.ComputeProgress;
                        }
                    }

                    // 获取总共启动的线程数量
                    int hashCount = 0;
                    foreach (bool fileHashEnable in fileHashEnables)
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
        protected virtual void OnCompleted(object sender, CompletedEventArgs e)
        {
            Completed?.Invoke(sender, e);
        }

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
                this.isNormallyCompleted = isNormallyCompleted;
            }

            /// <summary>
            /// 以是否正常完成标志和计算结果作为参数实例化此类
            /// </summary>
            /// <param name="isNormallyCompleted"></param>
            public CompletedEventArgs(bool isNormallyCompleted, FileHashParallel result)
            {
                this.isNormallyCompleted = isNormallyCompleted;
                this.result = result;
            }

            private bool isNormallyCompleted;
            private FileHashParallel result;

            /// <summary>
            /// 指示是否正常完成的标志位
            /// </summary>
            public bool IsNormallyCompleted { get => isNormallyCompleted; }
            /// <summary>
            /// 计算得到的散列值列表
            /// </summary>
            public FileHashParallel Result { get => result; }
        }
    }
}
