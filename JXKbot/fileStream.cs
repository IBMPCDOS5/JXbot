using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JXKbot
{
    public class fileStream
    {
        JXbot bot;
        public void Write(String toWrite)
        {
            StreamWriter write = new StreamWriter("warnings-" + bot.serverName + ".txt");
            write.WriteLine(toWrite);
            write.Close();
        }
    }
}
