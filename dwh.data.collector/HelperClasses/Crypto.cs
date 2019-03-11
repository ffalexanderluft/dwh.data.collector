using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;

namespace dwh.data.collector.Helperclasses
{
    class Crypto
    {
        private readonly byte[] KEY_64 = new byte[] { 42, 16, 93, 156, 78, 4, 218, 32 };
        private readonly byte[] IV_64 = new byte[] { 55, 103, 246, 79, 36, 99, 167, 3 };
        
        public string Encrypt(string value = "")
        {
            try
            {
                if (value != "")
                {
                    DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
                    MemoryStream ms = new MemoryStream();
                    CryptoStream cs = new CryptoStream(ms, cryptoProvider.CreateEncryptor(KEY_64, IV_64), CryptoStreamMode.Write);
                    StreamWriter sw = new StreamWriter(cs);

                    sw.Write(value);
                    sw.Flush();
                    cs.FlushFinalBlock();
                    ms.Flush();

                    int l = Convert.ToInt32(ms.Length);
                    return Convert.ToBase64String(ms.GetBuffer(), 0, l);


                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return string.Format("Error: {0}", ex.Message);
            }
        }

        public string Decrypt(string value)
        {
            try
            {
                if (value != "")
                {
                    DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
                    byte[] buffer = Convert.FromBase64String(value);
                    MemoryStream ms = new MemoryStream(buffer);
                    CryptoStream cs = new CryptoStream(ms, cryptoProvider.CreateDecryptor(KEY_64, IV_64), CryptoStreamMode.Read);
                    StreamReader sr = new StreamReader(cs);

                    return sr.ReadToEnd();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return string.Format("Error: {0}", ex.Message);
            }
        }

        public string HashMD5(string value, Encoding encoding)
        {
            try
            {
                if (value != "")
                {
                    byte[] input = encoding.GetBytes(value);
                    byte[] hash = (new MD5CryptoServiceProvider()).ComputeHash(input);
                    return encoding.GetString(hash);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return string.Format("Error: {0}", ex.Message);
            }
        }


    }



}
