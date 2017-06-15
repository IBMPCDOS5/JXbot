/*
MIT License
Copyright (c) JayXKanz666 2017
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using Discord;
using JXbot.Modules.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JXbot.Extensions
{
    public static class EvalExtension
    {
        public static double Operator(this string logic, double x, double y)
        {
            switch (logic)
            {
                case "+": return x + y;
                case "-": return x - y;
                case "/": return x / y;
                case "pi": return x * Math.PI;
                case "*": return x * y;
                case "sqrt": return Math.Sqrt(x);
                case "^": return Math.Pow(x, y);
                case "tan": return Math.Tan(x);
                case "sin": return Math.Sin(x);
                default: return -1;
            }
        }

        public static List<double> FindNumList(string numlist)
        {
            List<double> list = new List<double>();

            list = Regex.Matches(numlist, @"[0-9]+(\.[0-9]+)?").OfType<Match>().Select(m => double.Parse(m.Value)).ToList();

            return list;
        }

        public static double eval(string equation)
        {
            var num = FindNumList(equation);
            var output = 0d;
            bool firstOp = true;

            for (int i = 0; i < num.Count - 1; i++)
            {
                var currentNum = num[i];
                var nextNum = num[i + 1];
                if (equation.Contains(currentNum.ToString() + " * " + nextNum.ToString()))
                {
                    if (firstOp) { output = currentNum * nextNum; firstOp = false; }
                    else output = output * nextNum;
                }
                else if (equation.Contains(currentNum.ToString() + " + " + nextNum.ToString()))
                {
                    if (firstOp) { output = currentNum + nextNum; firstOp = false; }
                    else output = output + nextNum;
                }
                else if (equation.Contains(currentNum.ToString() + " / " + nextNum.ToString()))
                {
                    if (firstOp) { output = currentNum / nextNum; firstOp = false; }
                    else output = output / nextNum;
                }
                else if (equation.Contains(currentNum.ToString() + " ^ " + nextNum.ToString()))
                {
                    if (firstOp) { output = Math.Pow(currentNum, nextNum); firstOp = false; }
                    else output = Math.Pow(output, nextNum);
                }
                else if (equation.Contains(currentNum.ToString() + " - " + nextNum.ToString()))
                {
                    if (firstOp) { output = currentNum - nextNum; firstOp = false; }
                    else output = output - nextNum;
                }
                else
                {
                    return -1;
                }
            }
            return output;
        }
    }
}
