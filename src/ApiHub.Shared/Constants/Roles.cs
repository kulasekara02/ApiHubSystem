namespace ApiHub.Shared.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Analyst = "Analyst";
    public const string Viewer = "Viewer";

    public static readonly string[] All = { Admin, Analyst, Viewer };
}
