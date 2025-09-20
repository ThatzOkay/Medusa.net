namespace Server.Utils;

public class LZ77
{
    public static byte[] Decompress(byte[] data)
    {
        // init a buffer, size comes from eamuemu
        var res = new byte[0x190000];
        // current data location
        var p = 0;
        // current output location
        var r = 0;
        // traceback location
        var t = 0;
        // bitmask location
        var b = 8; // read next bitmask byte on start
        byte mask = 0;

        while(true)
        {
            // read bitmask on block end
            if(b == 8)
            {
                mask = data[p];
                p += 1;
                b = 0;
            }
            // get mask for next byte
            if((mask & 1) == 1)
            { // not coded
                // copy the byte from data to result
                res[r] = data[p];
                r += 1;
                p += 1;
            }
            else
            { // coded
                // read descriptors
                int distance = data[p];
                int count = data[p + 1];
                // EOF mark
                if(distance == 0 && count == 0)
                {
                    break;
                }
                p += 2;
                // shift to correct location
                distance <<= 4;
                distance |= count >> 4;
                count = (count & 0x0F) + 3;
                // copy earlier result bytes to the end
                t = r - distance; // initialize traceback location
                for(var i = 0; i < count; i++)
                {
                    res[r] = t < 0 ? (byte)0x00 : res[t];
                    r += 1;
                    t += 1;
                }
            }
            // shift mask
            mask >>= 1;
            b += 1;
        }
        // r = result length
        var output = new byte[r];
        Array.Copy(res, output, r);
        return output;
    }

    public static byte[] CompressEmpty(byte[] data)
    {
        var res = new byte[data.Length + data.Length / 8 + 3];
        var p = 0;
        for(var i = 0; i < data.Length; i++)
        {
            if(i % 8 == 0)
            {
                if(data.Length - i < 8)
                {
                    res[p] = (byte)(Math.Pow(2, data.Length - i) - 1);
                }
                else
                {
                    res[p] = 255;
                }
                p += 1;
            }
            res[p] = data[i];
            p += 1;
        }
        res[p] = 0;
        res[p + 1] = 0;
        return res;
    }
}
