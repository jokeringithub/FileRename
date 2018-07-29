using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileRename
{
    /// <summary>
    /// 文件重命名类，可根据文件命名规则生成新文件名。
    /// </summary>
    public class NewFilename
    {
        /// <summary>
        /// 文件命名规则列表
        /// </summary>
        private readonly IList<NameRule> nameRuleList;
        /// <summary>
        /// 文件路径
        /// </summary>
        private readonly string filePath;
        /// <summary>
        /// 文件目前排序索引
        /// </summary>
        private readonly int fileIndex;

        /// <summary>
        /// 使用文件命名规则列表和文件目前排序实例化此类。
        /// </summary>
        /// <param name="nameRuleList">文件命名规则列表。</param>
        /// <param name="filePath">文件路径。</param>
        /// <param name="fileIndex">文件目前排序索引。</param>
        public NewFilename(IList<NameRule> nameRuleList, string filePath, int fileIndex)
        {
            this.nameRuleList = nameRuleList;
            this.filePath = filePath;
            this.fileIndex = fileIndex;
        }

        /// <summary>
        /// 获取文件的新文件名。
        /// </summary>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="OverflowException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public string NewName
        {
            get
            {
                string fileNewName = string.Empty;

                if ((filePath != null) && (filePath != string.Empty))
                {
                    foreach (NameRule nameRule in nameRuleList)
                    {
                        switch (nameRule.RuleType)
                        {
                            case NameRule.TypeCode.ConstantString:
                                fileNewName += GetConstanString(nameRule);
                                break;
                            case NameRule.TypeCode.OrderNumber:
                                // 若达到结束数限制，则返回空字符串
                                try
                                {
                                    fileNewName += GetOrderNumber(nameRule);
                                }
                                catch (ArgumentOutOfRangeException)
                                {
                                    return string.Empty;
                                }
                                break;
                            case NameRule.TypeCode.HashCode:
                                fileNewName += GetFileHashCode(nameRule);
                                break;
                            case NameRule.TypeCode.FileName:
                                fileNewName += GetFileNamePart(nameRule);
                                break;
                            case NameRule.TypeCode.Extension:
                                fileNewName += GetFileExtension(nameRule);
                                break;
                            default:
                                break;
                        }
                    }
                }

                return fileNewName;
            }
        }

        /// <summary>
        /// 获取文本部分。
        /// </summary>
        /// <param name="nameRule"></param>
        /// <returns></returns>
        private string GetConstanString(NameRule nameRule)
        {
            if ((nameRule.ConstantString != null) && (nameRule.ConstantString != string.Empty))
            {
                return nameRule.ConstantString;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取当前文件对应的顺序数。
        /// </summary>
        /// <param name="nameRule"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="OverflowException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private string GetOrderNumber(NameRule nameRule)
        {
            string orderNumberString;

            // 将各输入参数标准化。
            int numberLength, startNumber, endNumber;
            // 数字长度。
            if (nameRule.NumberLength <= 0)
            { numberLength = 0; }
            else
            { numberLength = nameRule.NumberLength; }
            // 开始数字。
            if ((nameRule.StartNumberString == null) || (nameRule.StartNumberString == string.Empty))
            { startNumber = 0; }
            else
            {
                startNumber = Convert.ToInt32(nameRule.StartNumberString);
                // 若数字超出位数限制，则置位位数允许的最大值。
                if ((numberLength > 0) && (startNumber > (int)Math.Pow(10, numberLength) - 1))
                { startNumber = (int)Math.Pow(10, numberLength) - 1; }
            }
            // 结束数字。
            if ((nameRule.EndNumberString == null) || (nameRule.EndNumberString == string.Empty))
            {
                if (numberLength == 0)
                { endNumber = int.MaxValue; }
                else
                { endNumber = (int)Math.Pow(10, numberLength) - 1; }
            }
            else
            {
                endNumber = Convert.ToInt32(nameRule.EndNumberString);
                // 若结束数字超出位数限制，则置位位数允许的最大值。
                if ((numberLength > 0) && (endNumber > (int)Math.Pow(10, numberLength) - 1))
                { endNumber = (int)Math.Pow(10, numberLength) - 1; }
            }
            // 小于 0 抛出异常
            if ((startNumber < 0) || (endNumber < 0))
            { throw new FormatException(); }

            // 正序编号。
            if (startNumber <= endNumber)
            {
                int orderNumber = startNumber + fileIndex;
                if (orderNumber <= endNumber)
                {
                    orderNumberString = orderNumber.ToString();
                    // 数字长度为定长则补足位数。
                    if (numberLength != 0)
                    {
                        while (orderNumberString.Length < numberLength)
                        { orderNumberString = "0" + orderNumberString; }
                    }
                }
                // 达到结束数字则抛出异常。
                else
                { throw new ArgumentOutOfRangeException(); }
            }
            //逆序编号。
            else
            {
                int orderNumber = startNumber - fileIndex;
                if (orderNumber >= endNumber)
                {
                    orderNumberString = orderNumber.ToString();
                    // 数字长度为定长则补足位数。
                    if (numberLength != 0)
                    {
                        while (orderNumberString.Length < numberLength)
                        { orderNumberString = "0" + orderNumberString; }
                    }
                }
                // 达到结束数字则抛出异常。
                else
                { throw new ArgumentOutOfRangeException(); }
            }

            return orderNumberString;
        }

        /// <summary>
        /// 获取文件散列值。
        /// </summary>
        /// <param name="nameRule"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        private string GetFileHashCode(NameRule nameRule)
        {
            using (FileHash.FileHashParallel fileHashParallel =
                new FileHash.FileHashParallel(filePath, nameRule.HashSelected))
            {
                fileHashParallel.StartAsync();
                while (!fileHashParallel.IsCompleted)
                { System.Threading.Thread.Sleep(10);  }
                return fileHashParallel.ToUpperHexStrings().
                    FirstOrDefault(hashString => hashString != string.Empty);
            }
        }

        /// <summary>
        /// 获取原文件部分文件名。
        /// </summary>
        /// <param name="nameRule"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private string GetFileNamePart(NameRule nameRule)
        {
            // 获取开始和结束的索引数
            int startIndex, endIndex;
            if ((nameRule.StartIndexString == "") || (nameRule.StartIndexString == null))
            { startIndex = 0; }
            else
            { startIndex = Convert.ToInt32(nameRule.StartIndexString) - 1; }
            if ((nameRule.EndIndexString == "") || (nameRule.EndIndexString == null))
            { endIndex = int.MaxValue; }
            else
            { endIndex = Convert.ToInt32(nameRule.EndIndexString) - 1; }

            // 获取文件名和去掉扩展名的文件名
            string fileName = filePath.Substring(filePath.LastIndexOf(@"\") + 1);
            string fileNameWithoutExtension;
            try
            { fileNameWithoutExtension = fileName.Remove(fileName.LastIndexOf(".")); }
            catch (Exception)
            { fileNameWithoutExtension = fileName; }

            // 文件结尾超限则等于去掉扩展名后的文件名的长度
            if (endIndex > fileNameWithoutExtension.Length)
            { endIndex = fileNameWithoutExtension.Length - 1; }

            // 获取文件名的一部分
            string fileNamePart = fileName.Substring(startIndex, (endIndex - startIndex) + 1);
            return fileNamePart;
        }

        /// <summary>
        /// 获取文件扩展名。
        /// </summary>
        /// <param name="nameRule"></param>
        /// <returns></returns>
        private string GetFileExtension(NameRule nameRule)
        {
            string fileExtension;
            if (nameRule.IsCustomExtension)
            {
                // 自动补足"."并去掉空格
                fileExtension = nameRule.CustomExtension;
                fileExtension = fileExtension.Replace(" ", "");
                if (!fileExtension.StartsWith("."))
                { fileExtension = "." + fileExtension; }
            }
            else
            {
                string fileName = filePath.Substring(filePath.LastIndexOf(@"\") + 1);
                try
                { fileExtension = fileName.Substring(fileName.LastIndexOf(".")); }
                // 若文件本身无扩展名则返回空字符串
                catch (Exception)
                { fileExtension = ""; }
            }
            return fileExtension;
        }
    }
}
