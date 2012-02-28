using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ModUpdater.Client.SelfUpdateManager
{
    static class Effects
    {
        public static void WriteLine(string message)
        {
            foreach (char c in message)
            {
                Console.Write(c);
                Thread.Sleep(50);
            }
            Console.WriteLine();
        }
    }
}
