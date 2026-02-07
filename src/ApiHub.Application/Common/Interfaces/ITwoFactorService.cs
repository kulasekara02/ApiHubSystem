namespace ApiHub.Application.Common.Interfaces;

public interface ITwoFactorService
{
    string GenerateSecretKey();
    string GenerateQrCodeUri(string email, string secretKey);
    bool ValidateCode(string secretKey, string code);
    List<string> GenerateRecoveryCodes(int count = 10);
}
