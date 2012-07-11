//    File:        Mod.cs
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

namespace ModUpdater.Client.Utility
{
    public class Mod
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string File { get; set; }
        public string Hash { get; set; }
        public long Size { get; set; }
        public string Description { get; set; }
        public byte[] Contents { get; set; }
        public string Identifier { get; set; }
        public bool Optional { get; set; }
        public List<string> Requires { get; set; }
        public List<Mod> RequiredBy { get; set; }
        protected bool disposed = false;

        public void BuildRequiredByList(List<Mod> lsm)
        {
            foreach (Mod m in lsm.ToArray())
            {
                if (m.Requires.Contains(Identifier))
                {
                    m.RequiredBy.Add(m);
                }
            }
        }
        public override string ToString()
        {
            return Name;
        }
        #region Dispose Code
        ~Mod()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Name = null;
                    Author = null;
                    Hash = null;
                    Description = null;
                    File = null;
                }
                Contents = null;
                disposed = true;
            }
        }
        #endregion
        
    }
}
