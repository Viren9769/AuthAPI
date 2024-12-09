using System.Security.Cryptography;

namespace AuthAPI.Helpers
{
    public class PasswordHasher
    {

        private static RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        private static readonly int SaltSize = 16;
        private static readonly int HashSize = 20;
        private static readonly int Iterations = 10000;

        public static string HashPassword(string password)
        {
            byte[] salt;
            rng.GetBytes(salt = new byte[SaltSize]);
            var key = new Rfc2898DeriveBytes(password, salt, Iterations);
            var hash = key.GetBytes(HashSize);

            var hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            var base64Hash = Convert.ToBase64String(hashBytes);
            return base64Hash;
        }
    
    public static bool VerifyPassword(string password, string base64Hash)
    {
        var hashabytes = Convert.FromBase64String(base64Hash);

        var salt = new byte[SaltSize];
        Array.Copy(hashabytes, 0, salt, 0, SaltSize);

        var key = new Rfc2898DeriveBytes(password, salt, Iterations);
        byte[] has = key.GetBytes(HashSize);

        for (var i = 0; i < HashSize; i++)
        {
            if (hashabytes[i + SaltSize] != has[i])
                return false;
        }

        return true;


    }
}
}
/*{
    private static RSACryptoServiceProvider rng = new RSACryptoServiceProvider();
    private static readonly int SaltSize = 16;
    private static readonly int HashSize = 20;
    private static readonly int Iterations = 10000;

    public static string HashPassword(string password)
    {
        byte[] salt;
        rng.GetBytes(salt = new byte[SaltSize]);
        var key = new Rfc2898DeriveBytes(password, salt, Iterations);
        var hash = key.GetBytes(HashSize);

        var hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);
        var base64Hash = Convert.ToBase64String(hashBytes);
        return base64Hash;
    }
}*/

