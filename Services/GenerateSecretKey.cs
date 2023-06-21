using System;
using System.Security.Cryptography;
using System.Text;

public static class SecretKeyGenerator
{
    public static string GenerateSecretKey()
    {
        // Generate a random 256-bit key
        byte[] keyBytes = new byte[32]; // 32 bytes = 256 bits
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(keyBytes);
        }

        // Convert the key bytes to a string
        string secretKey = Convert.ToBase64String(keyBytes);
        Console.WriteLine($"SecretKeyGenerator secret key: {secretKey}");
        return secretKey;
    }
}
