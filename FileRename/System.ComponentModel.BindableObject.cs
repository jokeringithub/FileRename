/// <summary>
/// 可绑定数据类型BindableObject及对应列表变量的更新方法。
/// </summary>

namespace System.ComponentModel
{
    /// <summary>
    /// 可绑定对象，用于实现数据绑定到用户控件的抽象类。对INotifyPropertyChanged接口的简易实现。
    /// </summary>
    public abstract class BindableObject : INotifyPropertyChanged
    {
        /// <summary>
        /// 属性改变事件，当属性的set使用SetProperty(property, value)实现时触发。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 触发属性改变事件。
        /// </summary>
        /// <param name="propertyName">属性名</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// 改变属性时使用的方法。
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="item">属性对应的字段名</param>
        /// <param name="value">填入value</param>
        /// <param name="propertyName">属性名（由编译器自动获取）</param>
        /// <example>
        /// public var Property { get => property; set => SetProperty(property, value); }
        /// </example>
        protected void SetProperty<T>(ref T item, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            item = value;
            OnPropertyChanged(propertyName);
            if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(item, value))
            {
                item = value;
                OnPropertyChanged(propertyName);
            }
        }
    }
}

namespace System.Collections.Generic
{
    /// <summary>
    /// 实现List的PropertyChanged事件触发，变量的类必须实现INotifyPropertyChanged接口。
    /// </summary>
    public abstract class BindableListUpdate
    {
        /// <summary>
        /// 触发PropertyChanged事件，以实现更新绑定目标。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">要触发更新的列表变量List</param>
        /// <returns>要触发更新的列表变量List</returns>
        public static List<T> Update<T>(List<T> list)
        {
            T[] items = new T[list.Count];
            list.CopyTo(items);
            list.Clear();
            List<T> tempList = new List<T>();
            foreach (var item in items)
            { tempList.Add(item); }
            return tempList;
        }
    }
}

namespace System.Collections
{
    /// <summary>
    /// 实现ArrayList的PropertyChanged事件触发，变量的类必须实现INotifyPropertyChanged接口。
    /// </summary>
    public abstract class BindableArrayListUpdate
    {
        /// <summary>
        /// 触发PropertyChanged事件，以实现更新绑定目标。
        /// </summary>
        /// <param name="list">要触发更新的数组列表变量ArrayList</param>
        /// <returns>要触发更新的数组列表变量ArrayList</returns>
        public static ArrayList Update(ArrayList list)
        {
            Array items = new Array[list.Count];
            list.CopyTo(items);
            list.Clear();
            ArrayList tempList = new ArrayList();
            foreach (var item in items)
            { tempList.Add(item); }
            return tempList;
        }
    }
}
