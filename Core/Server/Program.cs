using Abstractions.Entities;
using Abstractions.Services;
using Abstractions.Utils;
using KbinXml.Net;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server;
using Server.Api;
using Server.Authentication;
using Server.Extensions;
using Server.Middlewares;
using Server.Models.Request;
using Server.Services;
using Server.Utils;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Scalar.AspNetCore;
using Server.GraphQL;
using Server.Plugins;

var key =
    Convert.FromHexString("00000000000069D74627D985EE2187161570D08D93B12455035B6DF0D8205DF5");

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
var logger = loggerFactory.CreateLogger("MedusaLogger");

builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<User>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddHandlers();

if (!File.Exists("database/Medusa.db"))
{
    Directory.CreateDirectory("database");
    await using (File.Create("database/Medusa.db"))
    {
    }
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=database/Medusa.db;"));

builder.Services.AddIdentityCore<User>(config =>
{
    config.Password.RequiredLength = 8;
    config.SignIn.RequireConfirmedEmail = false;
    config.Lockout.AllowedForNewUsers = true;
}).AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddMemoryCache();

var graphQlService = builder.Services.AddGraphQLServer();

graphQlService
    .RegisterDbContextFactory<AppDbContext>()
    .AddAuthorization()
    .AddQueryType<Query>()
    .AddTypeExtensionsInNamespaceOf<Query>()
    .AddMutationType<Mutation>()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .UseAutomaticPersistedOperationPipeline()
    .AddInMemoryOperationDocumentStorage()
    .AddCacheControl()
    .ModifyCostOptions(o =>
    {
        o.MaxFieldCost = 50000;
        o.MaxTypeCost = 50000;
    });
;

builder.Services.AddOpenApi("v1");

// Create plugin infrastructure manually so DiscoverPluginsAsync can call
// OnBuilderInitialize before the DI container is sealed by builder.Build().
var pluginRegistry = new PluginRegistry();
var pluginService = new PluginService(logger, pluginRegistry);
builder.Services.AddSingleton(pluginRegistry);
builder.Services.AddSingleton<IPluginService>(pluginService);
builder.Services.AddHostedService<PluginWatcher>();

await pluginService.DiscoverPluginsAsync(builder);

builder.Services.AddTransient<ICardService, CardService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddSingleton<IXmlLogService, XmlLogService>();

var app = builder.Build();

pluginService.SetServiceProvider(app.Services);
await pluginService.ActivatePluginsAsync(app);

app.Lifetime.ApplicationStarted.Register(() =>
{
    var serverAddresses = app.Services.GetRequiredService<IServer>()
        .Features.Get<IServerAddressesFeature>();

    var localIp = IpUtils.GetLocalIPv4();

    if (serverAddresses == null || localIp == null) return;
    foreach (var address in serverAddresses.Addresses)
    {
        var uri = new Uri(address);
        var displayAddress = Environment.GetEnvironmentVariable("MAIN_ADDRESS") ??
                             $"{uri.Scheme}://{localIp}:{uri.Port}";
        Console.WriteLine($"Accessible at: {displayAddress}");
        Console.WriteLine($"EAmuse accessible at: {displayAddress}/eamuse");
    }
});

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.

//app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference(o => o.AddDocument("v1"));
    app.MapOpenApi();
}

app.UseMiddleware<BodyParsingMiddleware>();
app.UseHandlers();

var apiGroup = app.MapGroup("/api").WithTags("API");
apiGroup.MapCardsApiEndpoints();
apiGroup.MapUserApiEndpoints();
apiGroup.MapIdentityApi();

var eamuseGroup = app.MapGroup("eamuse");

eamuseGroup.MapPost("/{model}/{module}/{method}", (string model, string module, string method,
    HttpContext httpContext, [FromServices] ILogger<Program> logger, [FromServices] IHandlerService handlerService,
    [FromServices] IPluginService pluginService) =>
{
    var amusementRequest = new AmusementRequest() { Model = model, Module = module ?? "", Method = method ?? "" };

    return HandleEAmuseRoute(amusementRequest, httpContext, logger, handlerService, pluginService);
});

eamuseGroup.MapPost("/{m}", (string m, [FromQuery] string model, [FromQuery] string? module, [FromQuery] string? method,
    [FromQuery] string? f,
    HttpContext httpContext, [FromServices] ILogger<Program> logger, [FromServices] IHandlerService handlerService,
    [FromServices] IPluginService pluginService) =>
{
    var amusementRequest = BuildAmusementRequest(model, module, method, f);

    return HandleEAmuseRoute(amusementRequest, httpContext, logger, handlerService, pluginService);
});

eamuseGroup.MapPost("/", ([FromQuery] string model, [FromQuery] string? module, [FromQuery] string? method,
    [FromQuery] string? f,
    HttpContext httpContext, [FromServices] ILogger<Program> logger, [FromServices] IHandlerService handlerService,
    [FromServices] IPluginService pluginService) =>
{
    var amusementRequest = BuildAmusementRequest(model, module, method, f);

    return HandleEAmuseRoute(amusementRequest, httpContext, logger, handlerService, pluginService);
});

var graphqlMap = app.MapGraphQL();

app.MapFallbackToFile("/index.html");


await using var scope = app.Services.CreateAsyncScope();

var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

await appDbContext.Database.MigrateAsync();

app.Run();
return;

AmusementRequest BuildAmusementRequest(string model, string? module, string? method, string? f)
{
    var amusementRequest = new AmusementRequest() { Model = model, Module = module ?? "", Method = method ?? "" };

    if (string.IsNullOrEmpty(f)) return amusementRequest;

    var fParts = f.Split('.');
    amusementRequest.Module = fParts[0];
    amusementRequest.Method = fParts[1];

    return amusementRequest;
}

async Task<IResult> HandleEAmuseRoute(AmusementRequest amusementRequest, HttpContext httpContext,
    ILogger<Program> logger, IHandlerService handlerService, IPluginService pluginService)
{
    Console.WriteLine(httpContext.Request.Headers.UserAgent);
    // Enable buffering to allow multiple reads of the request body
    httpContext.Request.EnableBuffering();
    var body = "";
    // The body is 932 encoded xml
    if (httpContext.Request.Body.Length != 0)
    {
        using var reader = new StreamReader(httpContext.Request.Body, Encoding.GetEncoding(932), false, 1024, true);
        body = await reader.ReadToEndAsync();
    }

    httpContext.Request.Body.Position = 0;

    var compress = httpContext.Request.Headers["X-Compress"].ToString().Contains("lz77");
    var encrypt = httpContext.Request.Headers["X-Eamuse-Info"].FirstOrDefault() is not null;

    var encoding = httpContext.Items["Encoding"]?.ToString() ?? "SHIFT_JIS";

    httpContext.Request.Headers.TryGetValue("IsEncoded", out var isEncoded);

    var originalInfo = httpContext.Request.Headers["X-Eamuse-Info"].FirstOrDefault() ?? "";

    httpContext.Response.Headers.Append("X-Eamuse-Info", originalInfo);
    httpContext.Response.Headers.Append("X-Compress", compress ? "lz77" : "none");
    httpContext.Response.Headers.Append("User-Agent", "EAMUSE.Httpac/1.0");

    var result = await HandleEAmuseRequest(amusementRequest, body, originalInfo, compress, encrypt, isEncoded == "true",
        encoding, logger, handlerService, pluginService);

    return TypedResults.Bytes(result, "application/octet-stream");
}

async Task<byte[]> HandleEAmuseRequest(AmusementRequest request, string body, string info, bool compress, bool encrypt,
    bool isEncoded, string encoding, ILogger<Program> logger, IHandlerService handlerService,
    IPluginService pluginService)
{
    logger.LogInformation("Handling {Module} {Method}", request.Module, request.Method);

    var document = new XDocument();

    if (!string.IsNullOrEmpty(body))
        document = XDocument.Parse(body);

    var responseXml = await handlerService.Handle(request.Model, request.Module, request.Method, document);

    var plugins = pluginService.GetPlugins();
    var forcedEncoding = plugins.FirstOrDefault(p => p.GameCode == request.Model.Split(":")[0])?.ForcedEncoding;

    var encodingEnum = encoding switch
    {
        "shift_jis" or "ShiftJIS" or "SHIFT_JIS" => KnownEncodings.ShiftJIS,
        "us-ascii" => KnownEncodings.ASCII,
        "utf-8" => KnownEncodings.UTF8,
        "euc-jp" => KnownEncodings.EUC_JP,
        "iso-8859-1" => KnownEncodings.ISO_8859_1,
        _ => throw new ArgumentException($"Unknown encoding: {encoding}")
    };

    if (forcedEncoding is not null)
    {
        encodingEnum = forcedEncoding.ToKnownEncoding();
    }

    byte[] encodedBody;

    if (!isEncoded)
    {
        var encoder = Encoding.GetEncoding(encoding);
        encodedBody = encoder.GetBytes(responseXml.ToString());
    }
    else
    {
        encodedBody = KbinConverter.Write(responseXml, encodingEnum);
    }

    if (compress)
    {
        encodedBody = LZ77.CompressEmpty(encodedBody);
    }

    var originalInfo = info.Split('-');

    if (!encrypt) return encodedBody;
    var part = Convert.FromHexString((originalInfo[1] + originalInfo[2]));
    for (var i = 0; i < 6; i++)
        key[i] = part[i];
    var rc4Key = MD5.HashData(key);
    encodedBody = RC4.Encrypt(rc4Key, encodedBody);

    return encodedBody;
}