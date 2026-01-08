namespace CreditCard.Infrastructure.Security;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
/// EF Core Value Converter for encrypting/decrypting sensitive string fields.
/// Uses AES-GCM authenticated encryption via IEncryptionService.
/// </summary>
public class EncryptedStringConverter : ValueConverter<string, string>
{
    public EncryptedStringConverter(IEncryptionService encryptionService)
        : base(
            v => encryptionService.Encrypt(v),
            v => encryptionService.Decrypt(v))
    {
    }
}
