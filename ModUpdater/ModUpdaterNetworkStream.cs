//    File:        ModUpdaterNetworkStream.cs
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
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace ModUpdater
{
    public class ModUpdaterNetworkStream : NetworkStream
    {
        public bool Disposed { get; protected set; }
        public byte[] Key { get; set; }
        public byte[] IV { get; set; }
        public bool Encrypted { get; set; }
        private Random r = new Random();

        public ModUpdaterNetworkStream(Socket s)
            : base(s)
        {
            Disposed = false;
            Encrypted = false;
            Key = GenerateKey();
            IV = GenerateIV();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Disposed = true;
        }
        protected byte[] GenerateKey()
        {
            byte[] ba = new byte[32];
            r.NextBytes(ba);
            return ba;
        }
        protected byte[] GenerateIV()
        {
            byte[] ba = new byte[16];
            r.NextBytes(ba);
            return ba;
        }
        public int ReadInt()
        {
            return IPAddress.HostToNetworkOrder((int)Read(4));
        }

        public short ReadShort()
        {
            return IPAddress.HostToNetworkOrder((short)Read(2));
        }

        public long ReadLong()
        {
            return IPAddress.HostToNetworkOrder((long)Read(8));
        }

        public double ReadDouble()
        {
            return new BinaryReader(this).ReadDouble();
        }

        public float ReadFloat()
        {
            return new BinaryReader(this).ReadSingle();
        }

        public Boolean ReadBoolean()
        {
            return new BinaryReader(this).ReadBoolean();
        }

        public byte[] ReadBytes(int count)
        {
            //if (Encrypted)
            //    return DecryptBytes(new BinaryReader(this).ReadBytes(count));
            //else
                return new BinaryReader(this).ReadBytes(count);
        }

        public String ReadString()
        {
            if (!Encrypted)
                return new BinaryReader(this).ReadString();
            else
                return ReadEncryptedString();
        }

        public String ReadEncryptedString()
        {
            return (AES_decrypt(new BinaryReader(this).ReadString()));

        }

        public void WriteString(String msg)
        {
            if (!Encrypted)
                new BinaryWriter(this).Write(msg);
            else
                WriteEncryptedString(msg);
        }

        public void WriteEncryptedString(String msg)
        {
            new BinaryWriter(this).Write(AES_encrypt(msg));
        }
        public void WriteInt(int i)
        {
            byte[] a = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(i));
            Write(a, 0, a.Length);
        }

        public void WriteLong(long i)
        {
            byte[] a = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(i));
            Write(a, 0, a.Length);
        }

        public void WriteShort(short i)
        {
            byte[] a = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(i));
            Write(a, 0, a.Length);
        }

        public void WriteDouble(double d)
        {
            new BinaryWriter(this).Write(d);
        }

        public void WriteFloat(float f)
        {
            new BinaryWriter(this).Write(f);
        }

        public void WriteBoolean(Boolean b)
        {
            new BinaryWriter(this).Write(b);
        }

        public void WriteBytes(byte[] b)
        {
            //if (Encrypted)
            //    new BinaryWriter(this).Write(EncryptBytes(b));
            //else
                new BinaryWriter(this).Write(b);
        }
        public Object Read(int num)
        {
            byte[] b = new byte[num];
            for (int i = 0; i < b.Length; i++)
            {
                b[i] = (byte)ReadByte();
            }
            switch (num)
            {
                case 4:
                    return BitConverter.ToInt32(b, 0);
                case 8:
                    return BitConverter.ToInt64(b, 0);
                case 2:
                    return BitConverter.ToInt16(b, 0);
                default:
                    return 0;
            }
        }
        private String AES_encrypt(String Input)
        {
            try
            {
                var aes = new RijndaelManaged();
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = Key;
                aes.IV = IV;

                var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
                byte[] xBuff = null;
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
                    {
                        byte[] xXml = Encoding.UTF8.GetBytes(Input);
                        cs.Write(xXml, 0, xXml.Length);
                    }

                    xBuff = ms.ToArray();
                }

                String Output = Convert.ToBase64String(xBuff);
                return Output;
            }
            catch (Exception e) { Console.WriteLine(e); throw e; }
        }
        private byte[] EncryptBytes(byte[] Input)
        {
            try
            {
                var aes = new RijndaelManaged();
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = Key;
                aes.IV = IV;

                var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
                byte[] xBuff = null;
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
                    {
                        cs.Write(Input, 0, Input.Length);
                    }

                    xBuff = ms.ToArray();
                }
                return xBuff;
            }
            catch (Exception e) { Console.WriteLine(e); throw e; }
        }
        private String AES_decrypt(String Input)
        {
            RijndaelManaged aes = new RijndaelManaged();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Key;
            aes.IV = IV;

            var decrypt = aes.CreateDecryptor();
            byte[] xBuff = null;
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
                {
                    byte[] xXml = Convert.FromBase64String(Input);
                    cs.Write(xXml, 0, xXml.Length);
                }

                xBuff = ms.ToArray();
            }

            String Output = Encoding.UTF8.GetString(xBuff);
            return Output;
        }
        private byte[] DecryptBytes(byte[] Input)
        {
            RijndaelManaged aes = new RijndaelManaged();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Key;
            aes.IV = IV;

            var decrypt = aes.CreateDecryptor();
            byte[] xBuff = null;
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
                {
                    cs.Write(Input, 0, Input.Length);
                }

                xBuff = ms.ToArray();
            }
            return xBuff;
        }

    }
}
