/*
MIT License
Copyright (c) JayXKanz666 2017
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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
                case "s": return index = 1;
                case "m": return index = 60;
                case "h": return index = 3600;
                default: //nothing 
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
