using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using KbinXml.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server;
using Server.Extensions;
using Server.Middlewares;
using Server.Services;
using Server.Utils;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Abstractions.Entities;
using Abstractions.Services;
using Abstractions.Utils;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Server.Authentication;
using Server.Models.Request;

var key =
    Convert.FromHexString("00000000000069D74627D985EE2187161570D08D93B12455035B6DF0D8205DF5");

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
var logger = loggerFactory.CreateLogger("MedusaLogger");

var pluginService = new PluginService(logger);

// Add services to the container.

pluginService.RegisterPlugins();

var plugins = pluginService.GetPlugins();

foreach (var plugin in plugins)
{
    await plugin.OnBuilderInitialize(builder);
}

builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<User>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddHandlers();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=Medusa.db;"));

builder.Services.AddIdentityCore<User>(config =>
{
    config.Password.RequiredLength = 8;
    config.SignIn.RequireConfirmedEmail = true;
    config.Lockout.AllowedForNewUsers = true;
}).AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddTransient<ICardService, CardService>();
builder.Services.AddSingleton<IPluginService>(pluginService);
builder.Services.AddSingleton<IXmlLogService, XmlLogService>();

var app = builder.Build();

app.Lifetime.ApplicationStarted.Register(() =>
{
    var serverAddresses = app.Services.GetRequiredService<IServer>()
        .Features.Get<IServerAddressesFeature>();

    var localIp = IpUtils.GetLocalIPv4();

    if (serverAddresses == null || localIp == null) return;
    foreach (var address in serverAddresses.Addresses)
    {
        var uri = new Uri(address);
        var displayAddress = $"{uri.Scheme}://{localIp}:{uri.Port}";
        Console.WriteLine($"Accessible at: {displayAddress}");
    }
});

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.

//app.UseHttpsRedirection();
app.UseMiddleware<BodyParsingMiddleware>();
app.UseHandlers();

foreach (var plugin in plugins)
{
    await plugin.OnAppInitialize(app);
}

app.MapIdentityApi();

var eamuseGroup = app.MapGroup("eamuse");

eamuseGroup.MapPost("/{model}/{module}/{method}", (string model, string module, string method,
    [FromServices] IHttpContextAccessor httpContextAccessor) =>
{

});

eamuseGroup.MapPost("/{m}", async (string m, [FromQuery] string model, [FromQuery] string? module, [FromQuery] string? method, [FromQuery] string? f,
    HttpContext httpContext, [FromServices] ILogger<Program> logger, [FromServices] IHandlerService handlerService) =>
{
    // Enable buffering to allow multiple reads of the request body
    httpContext.Request.EnableBuffering();

    // The body is 932 encoded xml
    using var reader = new StreamReader(httpContext.Request.Body, Encoding.GetEncoding(932), false, 1024, true);
    var body = await reader.ReadToEndAsync();

    httpContext.Request.Body.Position = 0;

    var compress = httpContext.Request.Headers["X-Compress"].ToString().Contains("lz77");
    var encrypt = httpContext.Request.Headers["X-Eamuse-Info"].FirstOrDefault() is not null;

    var amusementRequest = new AmusementRequest() { Model = model, Module = module ?? "", Method = method ?? "" };

    if(!string.IsNullOrEmpty(f))
    {
        var fParts = f.Split('.');
        amusementRequest.Module = fParts[0];
        amusementRequest.Method = fParts[1];
    }

    logger.LogInformation("Handling {Module} {Method}", amusementRequest.Module, amusementRequest.Method);

    var document = new XDocument();

    if (!string.IsNullOrEmpty(body))
        document = XDocument.Parse(body);

    var responseXml = await handlerService.Handle(amusementRequest.Model, amusementRequest.Module, amusementRequest.Method, document);

    var encoding = httpContext.Items["Encoding"]?.ToString() ?? "ShiftJIS";

    var encodingEnum = encoding switch
    {
        "shift_jis" or "ShiftJIS" or "SHIFT_JIS"  => KnownEncodings.ShiftJIS,
        "us-ascii"   => KnownEncodings.ASCII,
        "utf-8"      => KnownEncodings.UTF8,
        "euc-jp"     => KnownEncodings.EUC_JP,
        "iso-8859-1" => KnownEncodings.ISO_8859_1,
        _ => throw new ArgumentException($"Unknown encoding: {encoding}")
    };

    if (amusementRequest.Module == "package")
    {
        logger.LogInformation("{xml}", responseXml);
    }

    var encodedBody = KbinConverter.Write(responseXml, encodingEnum);
        
    if (amusementRequest.Module == "package")
    {
        File.WriteAllBytes("test.bin", encodedBody);
    }

    if(compress)
    {
        encodedBody = LZ77.CompressEmpty(encodedBody);
    }

    if (!encrypt) return TypedResults.Bytes(encodedBody, "application/octet-stream");
    var originalInfo = httpContext.Request.Headers["X-Eamuse-Info"].FirstOrDefault()?.Split('-') ?? [];
    var part = Convert.FromHexString((originalInfo[1] + originalInfo[2]));
    for(var i = 0; i < 6; i++)
        key[i] = part[i];
    var rc4Key = MD5.HashData(key);
    encodedBody = RC4.Encrypt(rc4Key, encodedBody);

    httpContext.Response.Headers.Append("X-Eamuse-Info", string.Join('-', originalInfo));

    return TypedResults.Bytes(encodedBody, "application/octet-stream");
});

eamuseGroup.MapPost("/", async ([FromQuery] string model, [FromQuery] string? module, [FromQuery] string? method, [FromQuery] string? f,
    HttpContext httpContext, [FromServices] ILogger<Program> logger, [FromServices] IHandlerService handlerService) =>
{
    // Enable buffering to allow multiple reads of the request body
    httpContext.Request.EnableBuffering();
    var body = "";
    // The body is 932 encoded xml
    if(httpContext.Request.Body.Length != 0)
    {
        using var reader = new StreamReader(httpContext.Request.Body, Encoding.GetEncoding(932), false, 1024, true);
        body = await reader.ReadToEndAsync();
    }

    httpContext.Request.Body.Position = 0;

    var compress = httpContext.Request.Headers["X-Compress"].ToString().Contains("lz77");
    var encrypt = httpContext.Request.Headers["X-Eamuse-Info"].FirstOrDefault() is not null;

    var amusementRequest = new AmusementRequest() { Model = model, Module = module ?? "", Method = method ?? ""};

    if (!string.IsNullOrEmpty(f))
    {
        var fParts = f.Split('.');
        amusementRequest.Module = fParts[0];
        amusementRequest.Method = fParts[1];
    }

    logger.LogInformation("Handling {Module} {Method}", amusementRequest.Module, amusementRequest.Method);

    var document = new XDocument();
    
    if(!string.IsNullOrEmpty(body))
        document = XDocument.Parse(body);

    var responseXml = await handlerService.Handle(amusementRequest.Model, amusementRequest.Module, amusementRequest.Method, document);

    var encoding = httpContext.Items["Encoding"]?.ToString() ?? "ShiftJIS";

    var encodingEnum = encoding switch
    {
        "shift_jis" or "ShiftJIS" or "SHIFT_JIS"  => KnownEncodings.ShiftJIS,
        "us-ascii"   => KnownEncodings.ASCII,
        "utf-8"      => KnownEncodings.UTF8,
        "euc-jp"     => KnownEncodings.EUC_JP,
        "iso-8859-1" => KnownEncodings.ISO_8859_1,
        _ => throw new ArgumentException($"Unknown encoding: {encoding}")
    };

    if (amusementRequest.Module == "package")
    {
        logger.LogInformation("{xml}", responseXml);
    }
    
    byte[] encodedBody;
    
    if (httpContext.Request.Headers.TryGetValue("IsEncoded", out var value) && value == "false")
    {
        var encoder = Encoding.GetEncoding(encoding);
        encodedBody = encoder.GetBytes(responseXml.ToString());
    }
    else
    {
        encodedBody = KbinConverter.Write(responseXml, encodingEnum);
        
        if (amusementRequest.Module == "package")
        {
            File.WriteAllBytes("test.bin", encodedBody);
        }
    }
    if(compress)
    {
        encodedBody = LZ77.CompressEmpty(encodedBody);
    }

    if (!encrypt) return TypedResults.Bytes(encodedBody, "application/octet-stream");
    var originalInfo = httpContext.Request.Headers["X-Eamuse-Info"].FirstOrDefault()?.Split('-') ?? [];
    var part = Convert.FromHexString((originalInfo[1] + originalInfo[2]));
    for(var i = 0; i < 6; i++)
        key[i] = part[i];
    var rc4Key = MD5.HashData(key);
    encodedBody = RC4.Encrypt(rc4Key, encodedBody);

    httpContext.Response.Headers.Append("X-Eamuse-Info", string.Join('-', originalInfo));

    return TypedResults.Bytes(encodedBody, "application/octet-stream");
});

app.MapFallbackToFile("/index.html");

await using var scope = app.Services.CreateAsyncScope();

var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
await appDbContext.Database.MigrateAsync();

app.Run();
return;

