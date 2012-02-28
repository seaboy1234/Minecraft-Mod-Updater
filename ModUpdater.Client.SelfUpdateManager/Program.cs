using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ModUpdater.Client.SelfUpdateManager
{
    class Program
    {
        static bool Update = false;
        static void Main(string[] args)
        {
#if DEBUG
            Update = true;
            File.WriteAllLines("testfile.txt.update", new string[] { "This is a test file", "Used for the updater tests.", "Congrats for finding this." });
            File.WriteAllLines("testfile.txt", new string[] { "This is a real file", "NOT used for the updater tests.", "Congrats for finding this." });
#endif
            if (args.Length > 0)
            {
                if (args[0] == "Security_Unlock_Code_Delta_Beta_7")
                    Update = true;
            }
            if (!Update)
            {
                Effects.WriteLine("File unlock code not provided.");
                Console.Read();
                return;
            }
            Effects.WriteLine("Security Unlock Code prossing...");
            Effects.WriteLine("Security Unlock Code DB7 was reconized as ACCESSLEVEL 5.  Starting Update...");
            Effects.WriteLine("Welcome to Update Mode.  Your update session will be encrypted and stored for future use.");
            
            foreach(string s in Directory.GetFiles(Environment.CurrentDirectory))
            {
                if (s.Contains(".update"))
                {
                    Effects.WriteLine("Updating " + Path.GetFileName(s.Replace(".update", "")));
                    File.Delete(s.Replace(".update", ""));
                    File.Move(s, s.Replace(".update", ""));
                    Effects.WriteLine(Path.GetFileName(s.Replace(".update", "")) + " Updated.");
                }
            }
            Effects.WriteLine("All files updated.  Press any key to launch.");
            Console.ReadKey();
            
        }
    }
}
