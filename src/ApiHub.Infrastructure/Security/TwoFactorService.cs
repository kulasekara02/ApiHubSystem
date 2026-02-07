using System.Security.Cryptography;
using System.Web;
using ApiHub.Application.Common.Interfaces;
using OtpNet;

namespace ApiHub.Infrastructure.Security;

public class TwoFactorService : ITwoFactorService
{
    private const string Issuer = "ApiHub";
    private const int SecretKeyLength = 20;

    public string GenerateSecretKey()
    {
        var key = KeyGeneration.GenerateRandomKey(SecretKeyLength);
        return Base32Encoding.ToString(key);
    }

    public string GenerateQrCodeUri(string email, string secretKey)
    {
        var encodedEmail = HttpUtility.UrlEncode(email);
        var encodedIssuer = HttpUtility.UrlEncode(Issuer);
        var encodedSecret = HttpUtility.UrlEncode(secretKey);

        return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={encodedSecret}&issuer={encodedIssuer}&algorithm=SHA1&digits=6&period=30";
    }

    public bool ValidateCode(string secretKey, string code)
    {
        if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        try
        {
            var keyBytes = Base32Encoding.ToBytes(secretKey);
            var totp = new Totp(keyBytes);

            // Allow for a time window of +/- 1 step (30 seconds each)
            return totp.VerifyTotp(code, out _, new VerificationWindow(previous: 1, future: 1));
        }
        catch
        {
            return false;
        }
    }

    public List<string> GenerateRecoveryCodes(int count = 10)
    {
        var codes = new List<string>();

        for (int i = 0; i < count; i++)
        {
            codes.Add(GenerateRecoveryCode());
        }

        return codes;
    }

    private static string GenerateRecoveryCode()
    {
        // Generate a random 8-character alphanumeric code in format XXXX-XXXX
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var bytes = new byte[8];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);

        var code = new char[8];
        for (int i = 0; i < 8; i++)
        {
            code[i] = chars[bytes[i] % chars.Length];
        }

        return $"{new string(code, 0, 4)}-{new string(code, 4, 4)}";
    }
}
