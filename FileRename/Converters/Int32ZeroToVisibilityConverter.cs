using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace XstarS.FileRename.Converters
{
    /// <summary>
    /// 表示 32 为整数为 0 到控件可见性 <see cref="Visibility"/> 的反转的转换器。
    /// </summary>
    [ValueConversion(typeof(int), typeof(Visibility))]
    internal class Int32ZeroToVisibilityConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value,
            Type targetType, object parameter, CultureInfo culture)
        {
            return ((value is int iValue) && (iValue == 0)) ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value,
            Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
