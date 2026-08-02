using static System.Net.Mime.MediaTypeNames;
using System.Text;
using System.Security.Cryptography;
using System.Xml.Linq;
using Server.Utils;
using KbinXml.Net;

namespace Server.Middlewares;

public class BodyParsingMiddleware(RequestDelegate next)
{
    private static readonly byte[] Key =
        Convert.FromHexString("00000000000069D74627D985EE2187161570D08D93B12455035B6DF0D8205DF5");

    private readonly RequestDelegate _next = next;

    public async Task Invoke(HttpContext context)
    {
        if(context.Request.Method == "GET" || !context.Request.Path.ToString().Contains("eamuse", StringComparison.CurrentCultureIgnoreCase))
        {
            await _next(context);
            return;
        }

        var body = await ParseRequest(context);

        context.Items["Encoding"] = body.Declaration?.Encoding;
        var bodyString = body.ToString();
        if(string.IsNullOrEmpty(bodyString))
        {
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(""));
            await _next(context);
            return;
        }
        context.Request.Body = new MemoryStream(Encoding.GetEncoding(932).GetBytes(bodyString));

        await _next(context);
    }

    public async Task<XDocument> ParseRequest(HttpContext context)
    {
        var isCompressed = context.Request.Headers["X-Compress"].ToString().Contains("lz77");
        var info = context.Request.Headers["X-Eamuse-Info"].FirstOrDefault();
        var contentLength = context.Request.Headers.ContentLength ?? 0;
        var data = new byte[(int)contentLength];

        await context.Request.Body.ReadExactlyAsync(data.AsMemory(0, (int)contentLength));

        if(info is not null)
        {
            var infoParts = info.Split('-');

            for(var i = 0; i < 6; i++)
            {
                Key[i] = Convert.ToByte((infoParts[1] + infoParts[2]).Substring(i << 1, 2), 0x10);
            }

            var rc4Key = MD5.HashData(Key);
            data = RC4.Decrypt(rc4Key, data);
        }

        if(isCompressed)
        {
            data = LZ77.Decompress(data);
        }

        var returnData = new XDocument();

        try
        {
            returnData = KbinConverter.ReadXmlLinq(data);
            context.Request.Headers.Append("IsEncoded", "true");
        }
        catch(Exception e)
        {
            //Console.WriteLine(e); 
            var testData = Encoding.ASCII.GetString(data);

            if (!string.IsNullOrEmpty(testData))
            {
                //Data is not konami encoded but raw xml
                returnData = XDocument.Parse(testData);
                context.Request.Headers.Append("IsEncoded", "false");
            }
        }
        //Data is now xml in konami binary form
        return returnData;
    }

}
