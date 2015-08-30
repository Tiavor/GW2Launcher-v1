using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Security.Cryptography;
/*
 * The MIT License (MIT)
 * 
 * Copyright © 2015, Tiavor
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 * 
 * Credits to Mud and Brainfloat on stackoverflow.com for providing this simple encryption
 * http://stackoverflow.com/a/5518092
 * http://stackoverflow.com/a/26177005
 */
namespace GW2Helper
{
    class StringCipher
    {
        private static readonly String salt1 = "aM4Yn0T", salt2 = "rbRSPiXNWqe0z5Csz", salt3="GymLTsPqyVYkVjdDGL53Xo";
        //just replace these with some random alphanumeric strings (size is not fixed)
        //i usualy take these from https://www.grc.com/passwords.htm
        //these aren't the salts i used for compiling ;)
    
        //encrypting the inputstring with a salted password
        //salt consists of logged in username, name of the computer and folder of this exe
        //+extra salt for the input, that is the password for the guildwars2 account
        internal static string Encrypt(string inputstring,string password)
        {
            ICryptoTransform encryptor;
            RijndaelManaged rm = new RijndaelManaged();
            //at each use a new init vector is generated, the result will never be the same twice
            byte[] IV=GenerateIV();
            string keybase = Environment.UserName + Environment.MachineName +
                System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            encryptor = rm.CreateEncryptor(generateKey(password+keybase), IV);
            //return ByteArrayToString(IV)+Convert.ToBase64String(Encrypt(StrToByteArray(salt1+inputstring),encryptor));
            //the /\ return method would work ... somehow, but it doesn't, so i converted all results into hex \/
            return ByteArrToHex(IV) + ByteArrToHex(Encrypt(StrToByteArray(salt3 + inputstring), encryptor));
            //attaching the init vector to the result
        }

        internal static string Decrypt(string encrypted,string account)
        {
            ICryptoTransform decryptor;
            RijndaelManaged rm = new RijndaelManaged();
            string keybase = Environment.UserName + Environment.MachineName+
                System.Reflection.Assembly.GetExecutingAssembly().CodeBase; //same as above
            decryptor = rm.CreateDecryptor(generateKey(account+keybase), getIV(encrypted));
            //getIV extracts the IV from the encrypted string
            //maybe it needs also a random disposable salt infront
            //or some kind of fixed password encrypting ... just to be sure it is save
            return ByteArrayToString(Decrypt(getEncryptText(encrypted), decryptor)).Substring(salt3.Length);
        }

        //////////////////
        // Key management
        internal static byte[] generateKey(string input)
        {
            byte[] byteInput = StrToByteArray(salt2+input+salt1);
            byte[] result;
            using (SHA256 shaM = new SHA256Managed())
            {
                result = shaM.ComputeHash(byteInput);
            }
            return result;
        }
        private static byte[] getIV(string input)
        {
            //string text = input.Substring(0, 8);
            //return StrToByteArray(text);

            string text = input.Substring(0, 32);
            return HexToByteArray(text);
        }
        private static byte[] getEncryptText(string input)
        {
            //string text = input.Substring(8);
            //return Convert.FromBase64String(text);

            string text = input.Substring(32);
            return HexToByteArray(text);
        }
        //not used for now, could be used to generate a random key each time or other stuff
        private static byte[] GenerateEncryptionKey()
        {
            RijndaelManaged rm = new RijndaelManaged();
            rm.GenerateKey();
            return rm.Key;
        }
        private static byte[] GenerateIV()
        {
            RijndaelManaged rm = new RijndaelManaged();
            rm.GenerateIV();
            return rm.IV;
        }

        //////////////////////////////////////////////
        // methods that are part of the en/decryption
        private static byte[] Encrypt(byte[] buffer,ICryptoTransform encryptor )
        {
            return Transform(buffer, encryptor);
        }

        private static byte[] Decrypt(byte[] buffer, ICryptoTransform decryptor)
        {
            return Transform(buffer, decryptor);
        }
    
        private static byte[] Transform(byte[] buffer, ICryptoTransform transform)
        {
            MemoryStream stream = new MemoryStream();
            using (CryptoStream cs = new CryptoStream(stream, transform, CryptoStreamMode.Write))
            {
                cs.Write(buffer, 0, buffer.Length);
            }
            return stream.ToArray();
        }
    
        ///////////////////
        // Utility Methods

        internal static byte[] StrToByteArray(string str)
        {
            System.Text.UnicodeEncoding encoding = new System.Text.UnicodeEncoding();
            return encoding.GetBytes(str);   
        }
        internal static string ByteArrayToString(byte[] byteArr)
        {
            System.Text.UnicodeEncoding encoding = new System.Text.UnicodeEncoding();
            return encoding.GetString(byteArr);
        }

        internal static byte[] HexToByteArray(string str)
        {
            int NumberChars = str.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(str.Substring(i, 2), 16);
            return bytes;
        }
    
        internal static string ByteArrToHex(byte[] byteArr)
        {
            StringBuilder result = new StringBuilder(byteArr.Length * 2);

            for (int i = 0; i < byteArr.Length; i++)
                result.Append(byteArr[i].ToString("x2"));
            return result.ToString();
        }
    }
}
