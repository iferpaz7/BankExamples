namespace CreditCard.Infrastructure.Security;

using CreditCard.Application.Interfaces;

/// <summary>
/// Adapter that implements IHashService using the IEncryptionService.
/// This maintains clean architecture boundaries.
/// </summary>
public class HashServiceAdapter : IHashService
{
    private readonly IEncryptionService _encryptionService;

    public HashServiceAdapter(IEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }

    public string ComputeHash(string value)
    {
        return _encryptionService.ComputeHash(value);
    }
}
