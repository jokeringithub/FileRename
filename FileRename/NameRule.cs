using System.ComponentModel;
using XstarS.ComponentModel;

namespace FileRename
{
    /// <summary>
    /// 文件重命名的命名规则类。
    /// </summary>
    public abstract partial class NameRule : INotifyPropertyChanged
    {
        /// <summary>
        /// 初始化 <see cref="NameRule"/> 的新实例。
        /// </summary>
        public NameRule()
        {
            this.RuleType = TypeCode.ConstantString;
            this.ConstantString = string.Empty;
            this.HashSelected = new bool[6];
            this.HashSelected[1] = true;
        }

        /// <summary>
        /// 命名规则类型。
        /// </summary>
        public abstract NameRule.TypeCode RuleType { get; set; }
        /// <summary>
        /// 固定字符串。
        /// </summary>
        public abstract string ConstantString { get; set; }
        /// <summary>
        /// 编号数字长度，为空或 0 则为可变长度。
        /// </summary>
        public abstract int NumberLength { get; set; }
        /// <summary>
        /// 起始编号，为空则默认为 0。
        /// </summary>
        public abstract string StartNumberString { get; set; }
        /// <summary>
        /// 结束编号，为空则默认最大。
        /// </summary>
        public abstract string EndNumberString { get; set; }
        /// <summary>
        /// 标识被选中的散列值。
        /// </summary>
        public abstract bool[] HashSelected { get; set; }
        /// <summary>
        /// 原文件名起始索引，为空默认为 0（对用户而言为 1）。
        /// </summary>
        public abstract string StartIndexString { get; set; }
        /// <summary>
        /// 原文件名结束索引，为空默认到结尾。
        /// </summary>
        public abstract string EndIndexString { get; set; }
        /// <summary>
        /// 指示是否使用自定义扩展名。
        /// </summary>
        public abstract bool IsCustomExtension { get; set; }
        /// <summary>
        /// 自定义扩展名。
        /// </summary>
        public abstract string CustomExtension { get; set; }

        /// <summary>
        /// 在属性值更改时发生。
        /// </summary>
        public abstract event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 使用命名规则类型 <see cref="NameRule.TypeCode"/> 创建一个 <see cref="NameRule"/> 的实例。
        /// </summary>
        /// <param name="ruleType">命名规则类型。</param>
        /// <returns>创建的 <see cref="NameRule"/> 的实例。</returns>
        public static NameRule Create(NameRule.TypeCode ruleType)
        {
            var value = BindableTypeProvider<NameRule>.Default.CreateInstance();
            value.RuleType = ruleType;
            return value;
        }
    }

    public abstract partial class NameRule
    {
        /// <summary>
        /// 用于指定命名规则的枚举类。
        /// </summary>
        public enum TypeCode
        {
            /// <summary>
            /// 固定的字符串。
            /// </summary>
            ConstantString,
            /// <summary>
            /// 顺序数。
            /// </summary>
            OrderNumber,
            /// <summary>
            /// 文件散列。
            /// </summary>
            HashCode,
            /// <summary>
            /// 来自原文件名。
            /// </summary>
            FileName,
            /// <summary>
            /// 文件扩展名。
            /// </summary>
            Extension
        }
    }
}
