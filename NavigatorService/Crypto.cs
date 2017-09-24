using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace WcfService1
{
    public class Crypto
    {
        public static int KEY_SIZE = 256;
        public static int BLOCK_SIZE = 128;

        //  Call this function to remove the key from memory after use for security
        [System.Runtime.InteropServices.DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(IntPtr Destination, int Length);

        public static byte[] GenerateKey(string keyOutputFilepath)
        {
            Guid salt = Guid.NewGuid();
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            FileStream fsCrypt = new FileStream(keyOutputFilepath, FileMode.Create);

            var key = new Rfc2898DeriveBytes(salt.ToByteArray(), saltBytes, 1000);
            byte[] _key = key.GetBytes(KEY_SIZE / 8);
            byte[] _iv = key.GetBytes(BLOCK_SIZE / 8);
            byte[] result = new byte[(KEY_SIZE / 8) + (BLOCK_SIZE / 8)];
            Array.Copy(_key, 0, result, 0, _key.Length);
            Array.Copy(_iv, 0, result, KEY_SIZE / 8, _iv.Length);

            fsCrypt.Write(result, 0, result.Length);
            fsCrypt.Flush();
            fsCrypt.Close();

            // Use the Automatically generated key for Encryption. 
            return result;
        }

        public static void AES_Encrypt(string inputFile, string outputFile, byte[] complexKeyBytes)
        {
            string cryptFile = outputFile;
            FileStream fsCrypt = new FileStream(cryptFile, FileMode.Create);

            RijndaelManaged AES = new RijndaelManaged();

            AES.KeySize = KEY_SIZE;
            AES.BlockSize = BLOCK_SIZE;

            byte[] _key = new byte[KEY_SIZE / 8];
            byte[] _iv = new byte[BLOCK_SIZE / 8];
            Array.Copy(complexKeyBytes, 0, _key, 0, KEY_SIZE / 8);
            Array.Copy(complexKeyBytes, KEY_SIZE / 8, _iv, 0, BLOCK_SIZE / 8);

            AES.Key = _key;
            AES.IV = _iv;
            AES.Padding = PaddingMode.Zeros;

            AES.Mode = CipherMode.CBC;

            CryptoStream cs = new CryptoStream(fsCrypt,
                 AES.CreateEncryptor(),
                CryptoStreamMode.Write);

            FileStream fsIn = new FileStream(inputFile, FileMode.Open);

            int data;
            while ((data = fsIn.ReadByte()) != -1)
                cs.WriteByte((byte)data);

            fsIn.Flush();
            fsIn.Close();
            cs.Flush();
            cs.Close();
            fsCrypt.Flush();
            fsCrypt.Close();

        }

        public static void AES_Decrypt(string inputFile, string outputFile, byte[] complexKeyBytes)
        {
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);

            RijndaelManaged AES = new RijndaelManaged();

            AES.KeySize = KEY_SIZE;
            AES.BlockSize = BLOCK_SIZE;


            byte[] _key = new byte[KEY_SIZE / 8];
            byte[] _iv = new byte[BLOCK_SIZE / 8];
            Array.Copy(complexKeyBytes, 0, _key, 0, KEY_SIZE / 8);
            Array.Copy(complexKeyBytes, KEY_SIZE / 8, _iv, 0, BLOCK_SIZE / 8);

            AES.Key = _key;
            AES.IV = _iv;
            AES.Padding = PaddingMode.Zeros;

            AES.Mode = CipherMode.CBC;

            CryptoStream cs = new CryptoStream(fsCrypt,
                AES.CreateDecryptor(),
                CryptoStreamMode.Read);

            FileStream fsOut = new FileStream(outputFile, FileMode.Create);

            int data;
            while ((data = cs.ReadByte()) != -1)
                fsOut.WriteByte((byte)data);

            fsOut.Flush();
            fsOut.Close();
            cs.Flush();
            cs.Close();
            fsCrypt.Flush();
            fsCrypt.Close();

        }

        public static void AES_Encrypt(string inputFile, string outputFile, string complexKeyFilepath)
        {
            byte[] complexKeyBytes = new byte[(KEY_SIZE / 8) + (BLOCK_SIZE / 8)];
            FileStream fsKey = new FileStream(complexKeyFilepath, FileMode.Open);
            fsKey.Read(complexKeyBytes, 0, (KEY_SIZE / 8) + (BLOCK_SIZE / 8));
            fsKey.Close();

            string cryptFile = outputFile;
            FileStream fsCrypt = new FileStream(cryptFile, FileMode.Create);

            RijndaelManaged AES = new RijndaelManaged();

            AES.KeySize = KEY_SIZE;
            AES.BlockSize = BLOCK_SIZE;

            byte[] _key = new byte[KEY_SIZE / 8];
            byte[] _iv = new byte[BLOCK_SIZE / 8];
            Array.Copy(complexKeyBytes, 0, _key, 0, KEY_SIZE / 8);
            Array.Copy(complexKeyBytes, KEY_SIZE / 8, _iv, 0, BLOCK_SIZE / 8);

            AES.Key = _key;
            AES.IV = _iv;
            AES.Padding = PaddingMode.Zeros;

            AES.Mode = CipherMode.CBC;

            CryptoStream cs = new CryptoStream(fsCrypt,
                 AES.CreateEncryptor(),
                CryptoStreamMode.Write);

            FileStream fsIn = new FileStream(inputFile, FileMode.Open);

            int data;
            while ((data = fsIn.ReadByte()) != -1)
                cs.WriteByte((byte)data);

            fsIn.Flush();
            fsIn.Close();
            cs.Flush();
            cs.Close();
            fsCrypt.Flush();
            fsCrypt.Close();

        }

        public static void AES_Decrypt(string inputFile, string outputFile, string complexKeyFilepath)
        {
            byte[] complexKeyBytes = new byte[(KEY_SIZE / 8) + (BLOCK_SIZE / 8)];
            FileStream fsKey = new FileStream(complexKeyFilepath, FileMode.Open);
            fsKey.Read(complexKeyBytes, 0, (KEY_SIZE / 8) + (BLOCK_SIZE / 8));
            fsKey.Close();

            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);

            RijndaelManaged AES = new RijndaelManaged();

            AES.KeySize = KEY_SIZE;
            AES.BlockSize = BLOCK_SIZE;


            byte[] _key = new byte[KEY_SIZE / 8];
            byte[] _iv = new byte[BLOCK_SIZE / 8];
            Array.Copy(complexKeyBytes, 0, _key, 0, KEY_SIZE / 8);
            Array.Copy(complexKeyBytes, KEY_SIZE / 8, _iv, 0, BLOCK_SIZE / 8);

            AES.Key = _key;
            AES.IV = _iv;
            AES.Padding = PaddingMode.Zeros;

            AES.Mode = CipherMode.CBC;

            CryptoStream cs = new CryptoStream(fsCrypt,
                AES.CreateDecryptor(),
                CryptoStreamMode.Read);

            FileStream fsOut = new FileStream(outputFile, FileMode.Create);

            int data;
            while ((data = cs.ReadByte()) != -1)
                fsOut.WriteByte((byte)data);

            fsOut.Flush();
            fsOut.Close();
            cs.Flush();
            cs.Close();
            fsCrypt.Flush();
            fsCrypt.Close();

        }

        public static void AES_Encrypt(byte[] inputFile, ref byte[] outputFile, byte[] complexKeyBytes)
        {

            RijndaelManaged AES = new RijndaelManaged();

            AES.KeySize = KEY_SIZE;
            AES.BlockSize = BLOCK_SIZE;

            byte[] _key = new byte[KEY_SIZE / 8];
            byte[] _iv = new byte[BLOCK_SIZE / 8];
            Array.Copy(complexKeyBytes, 0, _key, 0, KEY_SIZE / 8);
            Array.Copy(complexKeyBytes, KEY_SIZE / 8, _iv, 0, BLOCK_SIZE / 8);

            AES.Key = _key;
            AES.IV = _iv;
            AES.Padding = PaddingMode.Zeros;

            AES.Mode = CipherMode.CBC;

            var encryptor = AES.CreateEncryptor();
            using (var stream = new MemoryStream())
            using (var encrypt = new CryptoStream(stream, encryptor, CryptoStreamMode.Write))
            {
                encrypt.Write(inputFile, 0, inputFile.Length);
                encrypt.FlushFinalBlock();
                outputFile = stream.ToArray();
            }

        }

        public static void AES_Decrypt(byte[] inputFile, ref byte[] outputFile, byte[] complexKeyBytes)
        {
            byte[] _key = new byte[KEY_SIZE / 8];
            byte[] _iv = new byte[BLOCK_SIZE / 8];
            Array.Copy(complexKeyBytes, 0, _key, 0, KEY_SIZE / 8);
            Array.Copy(complexKeyBytes, KEY_SIZE / 8, _iv, 0, BLOCK_SIZE / 8);

            RijndaelManaged AES = new RijndaelManaged();

            AES.KeySize = KEY_SIZE;
            AES.BlockSize = BLOCK_SIZE;

            AES.Key = _key;
            AES.IV = _iv;
            AES.Padding = PaddingMode.Zeros;

            AES.Mode = CipherMode.CBC;

            var decryptor = AES.CreateDecryptor();

            using (var stream = new MemoryStream())
            using (var encrypt = new CryptoStream(stream, decryptor, CryptoStreamMode.Write))
            {
                encrypt.Write(inputFile, 0, inputFile.Length);
                encrypt.FlushFinalBlock();
                outputFile = stream.ToArray();
            }

        }
    }
}