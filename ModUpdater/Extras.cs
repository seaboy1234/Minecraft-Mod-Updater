using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Drawing;
using System.Drawing.Imaging;

namespace ModUpdater
{
    public static class Extras
    {
        public static string GenerateHash(string filePathAndName)
        {
            string hashText = "";
            string hexValue = "";

            byte[] fileData = File.ReadAllBytes(filePathAndName);
            byte[] hashData = SHA1.Create().ComputeHash(fileData); // SHA1 or MD5

            foreach (byte b in hashData)
            {
                hexValue = b.ToString("X").ToLower(); // Lowercase for compatibility on case-sensitive systems
                hashText += (hexValue.Length == 1 ? "0" : "") + hexValue;
            }
            Console.WriteLine(hashText);
            return hashText;
        }
        public static string GenerateHash(byte[] fileContents)
        {
            string hashText = "";
            string hexValue = "";

            byte[] fileData = fileContents;
            byte[] hashData = SHA1.Create().ComputeHash(fileData); // SHA1 or MD5

            foreach (byte b in hashData)
            {
                hexValue = b.ToString("X").ToLower(); // Lowercase for compatibility on case-sensitive systems
                hashText += (hexValue.Length == 1 ? "0" : "") + hexValue;
            }
            Console.WriteLine(hashText);
            return hashText;
        }
        public static Image ImageFromBytes(byte[] bytes)
        {
            Image picture = null;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                picture = Image.FromStream(ms);
                ms.Dispose();
            }
            return picture;
        }
        public static byte[] BytesFromImage(Image image)
        {
            byte[] bytes;
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);
                bytes = ms.ToArray();
                ms.Dispose();
            }
            return bytes;
        }
        public static byte[] ImageBytesFromFile(string path)
        {
            Image i = Image.FromFile(path);
            return BytesFromImage(i);
        }
    }
}
