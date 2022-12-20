using System;
using System.Security.Cryptography;
using System.Text;

class AES
{
    public static byte[] EncryptAES(byte[] input, string key)
    {
        byte[] KeyBytes = new UTF8Encoding().GetBytes(key);

        Aes AESImplementation = Aes.Create("AES");
        AESImplementation.Key = KeyBytes;
        AESImplementation.Mode = CipherMode.ECB;

        ICryptoTransform CryptoTransform = AESImplementation.CreateEncryptor();

        return CryptoTransform.TransformFinalBlock(input, 0, input.Length);
    }

    public static byte[] EncryptAES(string input, string key)
    {
        byte[] StringBytes = new UTF8Encoding().GetBytes(input);
        return EncryptAES(StringBytes, key);
    }

    public static string EncryptAESToString(byte[] input, string key)
    {
        return Convert.ToBase64String(EncryptAES(input, key));
    }

    public static string EncryptAESToString(string input, string key)
    {
        return Convert.ToBase64String(EncryptAES(input, key));
    }

    public static byte[] DecryptAES(byte[] input, string key)
    {
        byte[] KeyBytes = new UTF8Encoding().GetBytes(key);

        Aes AESImplementation = Aes.Create("AES");
        AESImplementation.Key = KeyBytes;
        AESImplementation.Mode = CipherMode.ECB;

        ICryptoTransform CryptoTransform = AESImplementation.CreateDecryptor();

        return CryptoTransform.TransformFinalBlock(input, 0, input.Length);
    }

    public static byte[] DecryptAESFromString(string input, string key)
    {
        return DecryptAES(Convert.FromBase64String(input), key);
    }

    public static string DecryptAESToString(byte[] input, string key)
    {
        return new UTF8Encoding().GetString(DecryptAES(input, key));
    }

    public static string DecryptAESToString(string input, string key)
    {
        return new UTF8Encoding().GetString(DecryptAESFromString(input, key));
    }
}