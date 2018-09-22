using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XstarS.ComponentModel;

namespace FileRename
{
    /// <summary>
    /// 文件重命名的命名规则类。
    /// </summary>
    public partial class NameRule : BindableObject
    {
        /// <summary>
        /// 命名规则类型。
        /// </summary>
        private NameRule.TypeCode ruleType;
        /// <summary>
        /// 固定字符串。
        /// </summary>
        private string constantString;
        /// <summary>
        /// 编号数字长度，为空或 0 则为可变长度。
        /// </summary>
        private int numberLength;
        /// <summary>
        /// 起始编号，为空则默认为 0。
        /// </summary>
        private string startNumberString;
        /// <summary>
        /// 结束编号，为空则默认最大。
        /// </summary>
        private string endNumberString;
        /// <summary>
        /// 标识被选中的散列值。
        /// </summary>
        private bool[] hashSelected;
        /// <summary>
        /// 原文件名起始索引，为空默认为 0（对用户而言为 1）。
        /// </summary>
        private string startIndexString;
        /// <summary>
        /// 原文件名结束索引，为空默认到结尾。
        /// </summary>
        private string endIndexString;
        /// <summary>
        /// 指示是否使用自定义扩展名。
        /// </summary>
        private bool isCustomExtension;
        /// <summary>
        /// 自定义扩展名。
        /// </summary>
        private string customExtension;

        /// <summary>
        /// 使用命名规则类型 <see cref="NameRule.TypeCode"/> 初始化 <see cref="NameRule"/> 的实例。
        /// </summary>
        /// <param name="ruleType">命名规则类型。</param>
        public NameRule(NameRule.TypeCode ruleType)
        {
            this.ruleType = ruleType;
            this.constantString = string.Empty;
            this.hashSelected = new bool[6];
            this.hashSelected[2] = true;
        }

        /// <summary>
        /// 命名规则类型。
        /// </summary>
        public NameRule.TypeCode RuleType
        {
            get => this.ruleType;
            set => this.SetProperty(ref this.ruleType, value);
        }
        /// <summary>
        /// 固定字符串。
        /// </summary>
        public string ConstantString
        {
            get => this.constantString;
            set => this.SetProperty(ref this.constantString, value);
        }
        /// <summary>
        /// 编号数字长度，为空或 0 则为可变长度。
        /// </summary>
        public int NumberLength
        {
            get => this.numberLength;
            set => this.SetProperty(ref this.numberLength, value);
        }
        /// <summary>
        /// 起始编号，为空则默认为 0。
        /// </summary>
        public string StartNumberString
        {
            get => this.startNumberString;
            set => this.SetProperty(ref this.startNumberString, value);
        }
        /// <summary>
        /// 结束编号，为空则默认最大。
        /// </summary>
        public string EndNumberString
        {
            get => this.endNumberString;
            set => this.SetProperty(ref this.endNumberString, value);
        }
        /// <summary>
        /// 标识被选中的散列值。
        /// </summary>
        public bool[] HashSelected
        {
            get => this.hashSelected;
            set => this.SetProperty(ref this.hashSelected, value);
        }
        /// <summary>
        /// 原文件名起始索引，为空默认为 0（对用户而言为 1）。
        /// </summary>
        public string StartIndexString
        {
            get => this.startIndexString;
            set => this.SetProperty(ref this.startIndexString, value);
        }
        /// <summary>
        /// 原文件名结束索引，为空默认到结尾。
        /// </summary>
        public string EndIndexString
        {
            get => this.endIndexString;
            set => this.SetProperty(ref this.endIndexString, value);
        }
        /// <summary>
        /// 指示是否使用自定义扩展名。
        /// </summary>
        public bool IsCustomExtension
        {
            get => this.isCustomExtension;
            set => this.SetProperty(ref this.isCustomExtension, value);
        }
        /// <summary>
        /// 自定义扩展名。
        /// </summary>
        public string CustomExtension
        {
            get => this.customExtension;
            set => this.SetProperty(ref this.customExtension, value);
        }
    }

    public partial class NameRule
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
