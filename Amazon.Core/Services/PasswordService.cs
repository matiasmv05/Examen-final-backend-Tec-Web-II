using Microsoft.Extensions.Options;
using Amazon.Core.CustomEntities;
using Amazon.Core.Interface;
using System.Security.Cryptography;


namespace Amazon.Core.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly PasswordOptions _options;
        public PasswordService(IOptions<PasswordOptions> options)
        {
            _options = options.Value;
        }

        public bool Check(string hash, string password)
        {
            //ITERACIONES . SALT . PASSWORD
            var parts = hash.Split('.');
            if (parts.Length != 3)
            {
                throw new FormatException("El formato del HASH es incorrecto");
            }

            var iterations = Convert.ToInt32(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var key = Convert.FromBase64String(parts[2]);

            //Implementacion del PWD
            byte[] keyToCheck = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                _options.KeySize
                );

            return CryptographicOperations.
                FixedTimeEquals(keyToCheck, key);
        }

        public string Hash(string password)
        {
            //ITERACIONES . SALT . PASSWORD

            //Generar random salt
            byte[] salt = RandomNumberGenerator.GetBytes(_options.SaltSize);

            //Implementacion del PWD
            byte[] key = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                _options.Iterations,
                HashAlgorithmName.SHA256,
                _options.KeySize
                );

            var keyBase64 = Convert.ToBase64String(key);
            var saltBase64 = Convert.ToBase64String(salt);

            return $"{_options.Iterations}.{saltBase64}.{keyBase64}";
        }
    }
}
