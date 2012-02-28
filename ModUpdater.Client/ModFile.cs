using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModUpdater.Client
{
    public class ModFile : IDisposable
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public byte[] FileContents { get; set; }
        public ModFile(string n, string f, int i)
        {
            Name = n;
            FileName = f;
            FileContents = new byte[i];
        }
        public void Dispose()
        {
            Name = null;
            FileName = null;
            FileContents = null;
        }
    }
}
