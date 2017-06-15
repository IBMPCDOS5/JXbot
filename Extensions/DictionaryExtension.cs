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
