using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModUpdater.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Server s = new Server();
            try
            {
                s.Start();
                Console.WriteLine("Server stopped.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Console.WriteLine("Press any key to close.");
                Console.ReadKey();
            }
        }
    }
}
