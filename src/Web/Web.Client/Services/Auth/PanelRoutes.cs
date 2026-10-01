using Core.Domain.Enums;

namespace Web.Client.Services.Auth;

public static class PanelRoutes
{
    public static string For(UserRole role) =>
        role == UserRole.Admin ? "/admin" : "/app";
}
