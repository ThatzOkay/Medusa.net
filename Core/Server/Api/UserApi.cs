using System.Security.Claims;

namespace Server.Api;

public static class UserApi
{
    public static RouteGroupBuilder MapUserApiEndpoints(this RouteGroupBuilder group)
    {
        var userGroup = group.MapGroup("/user").WithTags("User");

        userGroup.MapGet("/claims", GetClaims).Produces<Claim[]>(200).RequireAuthorization();
        return group;
    }

    internal static IResult GetClaims(HttpContext httpContext)
    {
        var user = httpContext.User;
        if(!(httpContext.User.Identity?.IsAuthenticated ?? false))
        {
            return Results.Unauthorized();
        }

        var claims = user.Claims.Select(c => new Claim(c.Type, c.Value)).ToArray();
        return Results.Ok(claims);
    }
}
