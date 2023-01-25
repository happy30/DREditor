using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System;

public static class EncryptionJSON
{
    public static string GenerateEncryptionKey()
    {
        char[] chars = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
        string keyResult = "";
        for (int i = 0; i < 8; i++)
        {
            keyResult += chars[UnityEngine.Random.Range(0, chars.Length)];
        }
        return keyResult;
    }

    public static string EncryptJSON(string jsonToEncrypt, string encryptionKey)
    {
        byte[] encryptData = ASCIIEncoding.ASCII.GetBytes(jsonToEncrypt);
        byte[] encryptKey = ASCIIEncoding.ASCII.GetBytes(encryptionKey);

        DESCryptoServiceProvider serviceProvider = new DESCryptoServiceProvider();
        MemoryStream memoryStream = new MemoryStream();
        CryptoStream cryptoStream = new CryptoStream(memoryStream, serviceProvider.CreateEncryptor(encryptKey, encryptKey), CryptoStreamMode.Write);
        cryptoStream.Write(encryptData, 0, encryptData.Length);
        cryptoStream.FlushFinalBlock();
        byte[] result = new byte[memoryStream.Length];
        memoryStream.Position = 0;
        memoryStream.Read(result, 0, result.Length);

        return Convert.ToBase64String(result);
    }

    public static string DecryptJSON(string jsonToDecrypt, string encryptionKey)
    {
        byte[] decryptData = Convert.FromBase64String(jsonToDecrypt);
        byte[] decryptKey = ASCIIEncoding.ASCII.GetBytes(encryptionKey);

        DESCryptoServiceProvider serviceProvider = new DESCryptoServiceProvider();
        MemoryStream memoryStream = new MemoryStream();
        CryptoStream cryptoStream = new CryptoStream(memoryStream, serviceProvider.CreateDecryptor(decryptKey, decryptKey), CryptoStreamMode.Write);
        cryptoStream.Write(decryptData, 0, decryptData.Length);
        cryptoStream.FlushFinalBlock();
        byte[] result = new byte[memoryStream.Length];
        memoryStream.Position = 0;
        memoryStream.Read(result, 0, result.Length);

        return ASCIIEncoding.ASCII.GetString(result);
    }
}