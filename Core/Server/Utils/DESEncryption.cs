using System.Security.Cryptography;

namespace Server.Utils;

public class DesEncryption(byte[] key)
{
    public byte[] Encrypt(byte[] data)
    {
        using var tripleDes = TripleDES.Create();

        tripleDes.Key = key;
        tripleDes.Mode = CipherMode.CBC;
        tripleDes.Padding = PaddingMode.None;
        tripleDes.IV = new byte[8];

        using var encryptor = tripleDes.CreateEncryptor();
        return encryptor.TransformFinalBlock(data, 0, data.Length);
    }

    public byte[] Decrypt(byte[] data)
    {
        using var tripleDES = TripleDES.Create();

        tripleDES.Key = key;
        tripleDES.Mode = CipherMode.CBC;
        tripleDES.Padding = PaddingMode.None;
        tripleDES.IV = new byte[8];

        using var decryptor = tripleDES.CreateDecryptor();
        return decryptor.TransformFinalBlock(data, 0, data.Length);
    }

}
