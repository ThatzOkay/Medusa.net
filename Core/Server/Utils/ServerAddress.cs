namespace Server.Utils;

public static class ServerAddress
{
    public static string PublicUrl =>
        Environment.GetEnvironmentVariable("MAIN_ADDRESS") ?? "http://localhost:5120";
}
