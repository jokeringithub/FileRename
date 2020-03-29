using System;
using System.Globalization;
using System.Windows.Data;
using XstarS.FileRename.Models;

namespace XstarS.FileRename.Converters
{
    /// <summary>
    /// 表示命名规则 <see cref="NamingRuleType"/> 到其本地化名称的转换器。
    /// </summary>
    [ValueConversion(typeof(NamingRuleType), typeof(string))]
    internal class NamingRuleTypeToStringConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value,
            Type targetType, object parameter, CultureInfo culture)
        {
            if (value is NamingRuleType nValue)
            {
                switch (nValue)
                {
                    case NamingRuleType.ConstantString: return "文本";
                    case NamingRuleType.OrderedNumber: return "数字";
                    case NamingRuleType.FileHash: return "哈希";
                    case NamingRuleType.FileName: return "文件名";
                    case NamingRuleType.Extension: return "扩展名";
                    default: return string.Empty;
                }
            }
            else { return null; }
        }

        /// <inheritdoc/>
        object IValueConverter.ConvertBack(object value,
            Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
