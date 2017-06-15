/*
MIT License
Copyright (c) JayXKanz666 2017
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JXbot.Extensions
{
    public class DictionaryExtension
    {
        public static Dictionary<ulong, bool> userSuggested = new Dictionary<ulong, bool>();
        public static Dictionary<ulong, int> currentState = new Dictionary<ulong, int>();
        public static Dictionary<ulong, bool> serverModeration = new Dictionary<ulong, bool>();
        public static Dictionary<string, int> pollVotes = new Dictionary<string, int>();
        public static Dictionary<ulong, Timer> nickTimeout = new Dictionary<ulong, Timer>();
        public static Dictionary<ulong, Timer> nickTimer = new Dictionary<ulong, Timer>();
        public static Dictionary<ulong, bool> userVoted = new Dictionary<ulong, bool>();
        public static Dictionary<int, int> votesPerOpt = new Dictionary<int, int>();
        public Dictionary<ulong, string> titleSug = new Dictionary<ulong, string>();
        public Dictionary<ulong, string> descSug = new Dictionary<ulong, string>();
        public Dictionary<ulong, string> messageHistory = new Dictionary<ulong, string>();
        public Dictionary<ulong, int> messageCount = new Dictionary<ulong, int>();
    }
}
