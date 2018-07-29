using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace FileRename
{
    namespace Converter
    {
        /// <summary>
        /// 选中索引值与是否有选中项目的转换器。
        /// </summary>
        [ValueConversion(typeof(int), typeof(bool))]
        public class SelectedIndexToSelectedConverter : IValueConverter
        {
            /// <summary>
            /// 选中索引值到是否有选中项目的转换。
            /// </summary>
            /// <param name="value"></param>
            /// <param name="targetType"></param>
            /// <param name="parameter"></param>
            /// <param name="culture"></param>
            /// <returns></returns>
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
                !((int)value == -1);

            /// <summary>
            /// 不支持此方法，总是抛出 <see cref="NotSupportedException"/> 异常。
            /// </summary>
            /// <param name="value"></param>
            /// <param name="targetType"></param>
            /// <param name="parameter"></param>
            /// <param name="culture"></param>
            /// <returns></returns>
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
                throw new NotSupportedException();
        }

        /// <summary>
        /// 项目数量与是否有项目的反转的转换器。
        /// </summary>
        [ValueConversion(typeof(int), typeof(bool))]
        public class ItemsCountToHasItemsInvertedConverter : IValueConverter
        {
            /// <summary>
            /// 项目数量到是否有项目的反转的转换。
            /// </summary>
            /// <param name="value"></param>
            /// <param name="targetType"></param>
            /// <param name="parameter"></param>
            /// <param name="culture"></param>
            /// <returns></returns>
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
                !((int)value == 0);

            /// <summary>
            /// 不支持此方法，总是抛出 <see cref="NotSupportedException"/> 异常。
            /// </summary>
            /// <param name="value"></param>
            /// <param name="targetType"></param>
            /// <param name="parameter"></param>
            /// <param name="culture"></param>
            /// <returns></returns>
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
                throw new NotSupportedException();
        }

        /// <summary>
        /// 整型逻辑到可见性的反转的转换器。
        /// </summary>
        [ValueConversion(typeof(int), typeof(Visibility))]
        public class IntLogicToVisibilityInvertedConverter : IValueConverter
        {
            /// <summary>
            /// 整型逻辑到可见性的反转的转换。
            /// </summary>
            /// <param name="value"></param>
            /// <param name="targetType"></param>
            /// <param name="parameter"></param>
            /// <param name="culture"></param>
            /// <returns></returns>
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
                ((int)value == 0) ? Visibility.Visible : Visibility.Hidden;

            /// <summary>
            /// 可见性到整型逻辑的反转的转换。
            /// </summary>
            /// <param name="value"></param>
            /// <param name="targetType"></param>
            /// <param name="parameter"></param>
            /// <param name="culture"></param>
            /// <returns></returns>
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
                ((Visibility)value == Visibility.Visible) ? 0 : 1;
        }

        /// <summary>
        /// 命名规则到命名规则名称的转换器。
        /// </summary>
        [ValueConversion(typeof(NameRule.TypeCode), typeof(string))]
        public class RuleTypeToStringConverter : IValueConverter
        {
            /// <summary>
            /// 命名规则到命名规则名称的转换。
            /// </summary>
            /// <param name="value"></param>
            /// <param name="targetType"></param>
            /// <param name="parameter"></param>
            /// <param name="culture"></param>
            /// <returns></returns>
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                switch ((NameRule.TypeCode)(value))
                {
                    case NameRule.TypeCode.ConstantString: return "文本";
                    case NameRule.TypeCode.OrderNumber: return "数字";
                    case NameRule.TypeCode.HashCode: return "散列";
                    case NameRule.TypeCode.FileName: return "文件名";
                    case NameRule.TypeCode.Extension: return "扩展名";
                    default: return "";
                }
            }

            /// <summary>
            /// 不支持此方法，总是抛出 <see cref="NotSupportedException"/> 异常。
            /// </summary>
            /// <param name="value"></param>
            /// <param name="targetType"></param>
            /// <param name="parameter"></param>
            /// <param name="culture"></param>
            /// <returns></returns>
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
                throw new NotSupportedException();
        }

        /// <summary>
        /// 命名规则到命名规则索引值的转换器。
        /// </summary>
        [ValueConversion(typeof(NameRule.TypeCode), typeof(int))]
        public class RuleTypeToIndexConverter : IValueConverter
        {
            /// <summary>
            /// 命名规则到命名规则索引值的转换。
            /// </summary>
            /// <param name="value"></param>
            /// <param name="targetType"></param>
            /// <param name="parameter"></param>
            /// <param name="culture"></param>
            /// <returns></returns>
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
                (int)((NameRule.TypeCode)(value));

            /// <summary>
            /// 命名规则索引值到命名规则的转换。
            /// </summary>
            /// <param name="value"></param>
            /// <param name="targetType"></param>
            /// <param name="parameter"></param>
            /// <param name="culture"></param>
            /// <returns></returns>
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
                (NameRule.TypeCode)((int)value);
        }
    }
}
