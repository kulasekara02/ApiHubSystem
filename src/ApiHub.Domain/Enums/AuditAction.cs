namespace ApiHub.Domain.Enums;

public enum AuditAction
{
    Login = 0,
    Logout = 1,
    Register = 2,
    PasswordReset = 3,
    ConnectorCreated = 10,
    ConnectorUpdated = 11,
    ConnectorDeleted = 12,
    ApiRequestSent = 20,
    ReportGenerated = 30,
    ReportDownloaded = 31,
    FileUploaded = 40,
    FileDeleted = 41,
    UserCreated = 50,
    UserUpdated = 51,
    UserDeleted = 52,
    RoleChanged = 53
}
