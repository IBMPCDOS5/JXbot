using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace JXbot.Common
{
    public class ReminderFormat
    {
        public int returnedArg(string arg)
        {
            var temp = Regex.Replace(arg, @"[\d-]", string.Empty);
            var index = 1;
            switch(temp.ToLower())
            {
                case "s":
                    index = (int)TimeValues.s;
                    break;
                case "m":
                    index = (int)TimeValues.m;
                    break;
                case "h":
                    index = (int)TimeValues.h;
                    break;
                default:
                    //nothing
                    break;
            }
            return index;
        }

        public int formattedInt(string arg)
        {
            var afterArg = arg.Remove(arg.Length - 1);

            return Convert.ToInt32(afterArg);
        }
    }
}
