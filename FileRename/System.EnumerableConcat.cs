/// <summary>
/// 提供了连接若干可列变量的方法。
/// </summary>

namespace System
{
    /// <summary>
    /// 提供连接可列变量（实现IEnumerable的类）的方法。
    /// </summary>
    public abstract class EnumerableConcat
    {
        /// <summary>
        /// 连接一维数组的方法。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arrays">要连接的各个一维数组</param>
        /// <returns>连接后的一维数组</returns>
        public static T[] ConcatArray<T>(params T[][] arrays)
        {
            System.Collections.Generic.List<T> output = new System.Collections.Generic.List<T>();
            for (int i = 0; i < arrays.Length; i++)
            {
                output.AddRange(arrays[i]);
            }
            return output.ToArray();
        }

        /// <summary>
        /// 连接列表（List）的方法。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arrays">要连接的各个列表</param>
        /// <returns>连接后的列表</returns>
        public static System.Collections.Generic.List<T> ConcatList<T>(params System.Collections.Generic.List<T>[] lists)
        {
            System.Collections.Generic.List<T> output = new System.Collections.Generic.List<T>();
            for (int i = 0; i < lists.Length; i++)
            {
                output.AddRange(lists[i]);
            }
            return output;
        }

        /// <summary>
        /// 连接数组列表（ArrayList）的方法。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arrays">要连接的各个数组列表</param>
        /// <returns>连接后的数组列表</returns>
        public static System.Collections.ArrayList ConcatList(params System.Collections.ArrayList[] lists)
        {
            System.Collections.ArrayList output = new System.Collections.ArrayList();
            for (int i = 0; i < lists.Length; i++)
            {
                output.AddRange(lists[i]);
            }
            return output;
        }
    }
}
