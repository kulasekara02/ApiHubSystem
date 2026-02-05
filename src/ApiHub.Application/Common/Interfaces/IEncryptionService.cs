namespace ApiHub.Application.Common.Interfaces;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
    string Hash(string input);
}
