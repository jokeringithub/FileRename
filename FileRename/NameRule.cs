using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileRename
{
    /// <summary>
    /// 文件重命名的命名规则类
    /// </summary>
    public class NameRule : BindableObject
    {
        /// <summary>
        /// 使用类型初始化命名规则类
        /// </summary>
        /// <param name="ruleType">命名规则类型</param>
        public NameRule(NameRuleType ruleType)
        {
            this.ruleType = ruleType;
            constantString = string.Empty;
            hashCodeSelected = new bool[6];
        }

        private NameRuleType ruleType;
        /// <summary>
        /// 命名规则类型
        /// </summary>
        public NameRuleType RuleType { get => ruleType; set => SetProperty(ref ruleType, value); }

        /// <summary>
        /// 用于指定命名规则的枚举类
        /// </summary>
        public enum NameRuleType
        {
            /// <summary>
            /// 固定的字符串
            /// </summary>
            ConstantString,
            /// <summary>
            /// 顺序数
            /// </summary>
            OrderNumber,
            /// <summary>
            /// 文件散列
            /// </summary>
            HashCode,
            /// <summary>
            /// 来自原文件名
            /// </summary>
            FileName,
            /// <summary>
            /// 文件扩展名
            /// </summary>
            Extension,
        }

        private string constantString;
        /// <summary>
        /// 用于NameRuleType.ConstantString的属性
        /// </summary>
        public string ConstantString { get => constantString; set => SetProperty(ref constantString, value); }

        private int numberLength;
        /// <summary>
        /// 编号数字长度，为空或0则为可变长度
        /// </summary>
        public int NumberLength { get => numberLength; set => SetProperty(ref numberLength, value); }

        private string startNumber;
        /// <summary>
        /// 起始编号，为空则默认为0
        /// </summary>
        public string StartNumber { get => startNumber; set => SetProperty(ref startNumber, value); }

        private string endNumber;
        /// <summary>
        /// 结束编号，为空则默认最大
        /// </summary>
        public string EndNumber { get => endNumber; set => SetProperty(ref endNumber, value); }

        private bool[] hashCodeSelected;
        /// <summary>
        /// 标识被选中的散列值
        /// </summary>
        public bool[] HashCodeSelected { get => hashCodeSelected; set => SetProperty(ref hashCodeSelected, value); }

        private string startIndex;
        /// <summary>
        /// 原文件名起始索引，为空默认为0（对用户而言为1）
        /// </summary>
        public string StartIndex { get => startIndex; set => SetProperty(ref startIndex, value); }

        private string endIndex;
        /// <summary>
        /// 原文件名结束索引，为空默认到结尾
        /// </summary>
        public string EndIndex { get => endIndex; set => SetProperty(ref endIndex, value); }

        private bool isCustomExtension;
        /// <summary>
        /// 指示是否使用自定义扩展名
        /// </summary>
        public bool IsCustomExtension { get => isCustomExtension; set => SetProperty(ref isCustomExtension, value); }

        private string customExtension;
        /// <summary>
        /// 自定义扩展名
        /// </summary>
        public string CustomExtension { get => customExtension; set => SetProperty(ref customExtension, value); }
    }
}
