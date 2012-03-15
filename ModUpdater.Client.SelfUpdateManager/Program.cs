//    File:        Program.cs
//    Copyright:   Copyright (C) 2012 Christian Wilson. All rights reserved.
//    Website:     https://github.com/seaboy1234/Minecraft-Mod-Updater
//    Description: This is intended to help Minecraft server owners who use mods make the experience of adding new mods and updating old ones easier for everyone.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
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
