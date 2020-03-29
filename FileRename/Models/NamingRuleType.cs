namespace XstarS.FileRename.Models
{
    /// <summary>
    /// 表示命名规则的类型。
    /// </summary>
    public enum NamingRuleType
    {
        /// <summary>
        /// 表示固定的字符串。
        /// </summary>
        ConstantString,
        /// <summary>
        /// 表示顺序编号值。
        /// </summary>
        OrderedNumber,
        /// <summary>
        /// 表示文件哈希值。
        /// </summary>
        FileHash,
        /// <summary>
        /// 表示原文件名。
        /// </summary>
        FileName,
        /// <summary>
        /// 表示文件扩展名。
        /// </summary>
        Extension
    }
}
