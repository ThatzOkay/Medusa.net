using System.Text;

namespace Server.Utils
{
    public class RC4
    {
        public static string Encrypt(string key, string data)
        {
            var unicode = Encoding.Unicode;

            return Convert.ToBase64String(Encrypt(unicode.GetBytes(key), unicode.GetBytes(data)));
        }

        public static string Decrypt(string key, string data)
        {
            var unicode = Encoding.Unicode;

            return unicode.GetString(Encrypt(unicode.GetBytes(key), Convert.FromBase64String(data)));
        }

        public static byte[] Encrypt(byte[] key, byte[] data)
        {
            return EncryptOutput(key, data).ToArray();
        }

        public static byte[] Decrypt(byte[] key, byte[] data)
        {
            return EncryptOutput(key, data).ToArray();
        }

        private static byte[] InitializeKeyStream(byte[] key)
        {
            var keyStream = Enumerable.Range(0, 256)
                .Select(i => (byte)i)
                .ToArray();

            for(int keyStreamIndex = 0, keyIndex = 0; keyStreamIndex < 256; keyStreamIndex++)
            {
                keyIndex = (keyIndex + key[keyStreamIndex % key.Length] + keyStream[keyStreamIndex]) & 255;

                Swap(keyStream, keyStreamIndex, keyIndex);
            }

            return keyStream;
        }

        private static IEnumerable<byte> EncryptOutput(byte[] key, IEnumerable<byte> data)
        {
            var keyStream = InitializeKeyStream(key);

            var keyStreamIndex = 0;
            var keyIndex = 0;

            return data.Select((dataByte) =>
            {
                keyStreamIndex = (keyStreamIndex + 1) & 255;
                keyIndex = (keyIndex + keyStream[keyStreamIndex]) & 255;

                Swap(keyStream, keyStreamIndex, keyIndex);

                return (byte)(dataByte ^ keyStream[(keyStream[keyStreamIndex] + keyStream[keyIndex]) & 255]);
            });
        }

        private static void Swap(byte[] keyStream, int firstIndex, int secondIndex) => (keyStream[secondIndex], keyStream[firstIndex]) = (keyStream[firstIndex], keyStream[secondIndex]);

    }
}
