using System;
using System.Globalization;
using System.Windows.Data;
using XstarS.FileRename.Models;

namespace XstarS.FileRename.Converters
{
    /// <summary>
    /// 命名规则类型 <see cref="NamingRuleType"/> 到命名规则索引值的转换器。
    /// </summary>
    [ValueConversion(typeof(NamingRuleType), typeof(int))]
    internal class NamingRuleTypeToIndexConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value,
            Type targetType, object parameter, CultureInfo culture)
        {
            return (value is NamingRuleType nValue) ? (int)nValue : 0;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value,
            Type targetType, object parameter, CultureInfo culture)
        {
            return (value is int iValue) ? (NamingRuleType)iValue : NamingRuleType.ConstantString;
        }
    }
}
