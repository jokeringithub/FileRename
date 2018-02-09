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
    /// <summary>
    /// 转换器命名空间
    /// </summary>
    namespace Converter
    {
        /// <summary>
        /// 自定义选中索引值与是否有选中项目的布尔值的转换器
        /// </summary>
        [ValueConversion(typeof(int), typeof(bool))]
        public class SelectedIndexToSelected : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if ((int)value == -1) { return false; }
                else { return true; }
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 自定义项目数量与是否有项目的布尔值的转换器
        /// </summary>
        [ValueConversion(typeof(int), typeof(bool))]
        public class ItemsCountToHasItems : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if ((int)value == 0) { return false; }
                else { return true; }
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 自定义整型逻辑到可见性的（反转的）转换器
        /// </summary>
        [ValueConversion(typeof(int), typeof(Visibility))]
        public class IntLogicToVisibilityInverted : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if ((int)value == 0) { return Visibility.Visible; }
                else { return Visibility.Hidden; }
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if ((Visibility)value == Visibility.Visible) { return 0; }
                else { return 1; }
            }
        }

        /// <summary>
        /// 命名规则到字符串的转换
        /// </summary>
        [ValueConversion(typeof(NameRule.NameRuleType), typeof(string))]
        public class RuleTypeToString : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                switch ((NameRule.NameRuleType)(value))
                {
                    case NameRule.NameRuleType.ConstantString: return "文本";
                    case NameRule.NameRuleType.OrderNumber: return "数字";
                    case NameRule.NameRuleType.HashCode: return "散列";
                    case NameRule.NameRuleType.FileName: return "文件名";
                    case NameRule.NameRuleType.Extension: return "扩展名";
                    default: return "";
                }
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 命名规则到索引的转换
        /// </summary>
        [ValueConversion(typeof(NameRule.NameRuleType), typeof(int))]
        public class RuleTypeToIndex : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return (int)((NameRule.NameRuleType)(value));
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return (NameRule.NameRuleType)((int)value);
            }
        }
    }
}
