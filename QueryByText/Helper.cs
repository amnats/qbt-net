using System;
using System.Collections.Generic;
using System.Text;

namespace QueryByText
{
    public static class Helper
    {
        public static string FirstToUppercase(string str)
        {
            return char.ToUpper(str[0]) + str.Substring(1);
        }
    }
}
