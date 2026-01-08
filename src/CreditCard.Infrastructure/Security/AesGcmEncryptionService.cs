namespace CreditCard.Infrastructure.Security;

using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Encryption service using AES-GCM (Galois/Counter Mode) which provides
/// authenticated encryption with associated data (AEAD).
/// This ensures both confidentiality and integrity (built-in MAC).
/// </summary>
public sealed class AesGcmEncryptionService : IEncryptionService, IDisposable
{
    private const int NonceSize = 12; // 96 bits - recommended for AES-GCM
    private const int TagSize = 16;   // 128 bits - authentication tag
    private const int KeySize = 32;   // 256 bits - AES-256

    private readonly byte[] _encryptionKey;
    private readonly byte[] _hmacKey;

    public AesGcmEncryptionService(IConfiguration configuration)
    {
        var encryptionKey = configuration["Encryption:Key"]
            ?? throw new InvalidOperationException("Encryption key is not configured. Please set 'Encryption:Key' in configuration.");
        
        var hmacKey = configuration["Encryption:HmacKey"]
            ?? throw new InvalidOperationException("HMAC key is not configured. Please set 'Encryption:HmacKey' in configuration.");

        _encryptionKey = DeriveKey(encryptionKey, KeySize);
        _hmacKey = DeriveKey(hmacKey, KeySize);
    }

    /// <summary>
    /// Constructor for testing purposes with explicit keys.
    /// </summary>
    public AesGcmEncryptionService(byte[] encryptionKey, byte[] hmacKey)
    {
        if (encryptionKey.Length != KeySize)
            throw new ArgumentException($"Encryption key must be {KeySize} bytes", nameof(encryptionKey));
        if (hmacKey.Length != KeySize)
            throw new ArgumentException($"HMAC key must be {KeySize} bytes", nameof(hmacKey));

        _encryptionKey = encryptionKey;
        _hmacKey = hmacKey;
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        var cipherText = new byte[plainBytes.Length];
        var tag = new byte[TagSize];

        using var aesGcm = new AesGcm(_encryptionKey, TagSize);
        aesGcm.Encrypt(nonce, plainBytes, cipherText, tag);

        // Combine: nonce + tag + ciphertext
        var result = new byte[NonceSize + TagSize + cipherText.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, result, NonceSize, TagSize);
        Buffer.BlockCopy(cipherText, 0, result, NonceSize + TagSize, cipherText.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        var fullCipher = Convert.FromBase64String(cipherText);

        if (fullCipher.Length < NonceSize + TagSize)
            throw new CryptographicException("Invalid ciphertext: too short");

        var nonce = new byte[NonceSize];
        var tag = new byte[TagSize];
        var cipherBytes = new byte[fullCipher.Length - NonceSize - TagSize];

        Buffer.BlockCopy(fullCipher, 0, nonce, 0, NonceSize);
        Buffer.BlockCopy(fullCipher, NonceSize, tag, 0, TagSize);
        Buffer.BlockCopy(fullCipher, NonceSize + TagSize, cipherBytes, 0, cipherBytes.Length);

        var plainBytes = new byte[cipherBytes.Length];

        using var aesGcm = new AesGcm(_encryptionKey, TagSize);
        aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }

    public string ComputeHash(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var valueBytes = Encoding.UTF8.GetBytes(value);
        using var hmac = new HMACSHA256(_hmacKey);
        var hash = hmac.ComputeHash(valueBytes);
        return Convert.ToBase64String(hash);
    }

    private static byte[] DeriveKey(string password, int keySize)
    {
        // Use PBKDF2 for key derivation from the configured password
        // Salt is fixed because we need deterministic key derivation
        // In production, consider storing the salt separately
        var salt = Encoding.UTF8.GetBytes("CreditCard.Encryption.Salt.V1");
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(keySize);
    }

    public void Dispose()
    {
        // Clear sensitive key material from memory
        CryptographicOperations.ZeroMemory(_encryptionKey);
        CryptographicOperations.ZeroMemory(_hmacKey);
    }
}
