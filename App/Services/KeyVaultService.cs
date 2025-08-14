using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using TestWpfApplication.Models;

namespace Wpf_Test_App.Services
{
    public class KeyVaultService
    {
        private AzureKeyVaultConfig _config;
        private static readonly MemoryCache _cache = MemoryCache.Default;
        private const string CacheKey = "MyRsaKey";

        public KeyVaultService(AzureKeyVaultConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<string> GetKey()
        {
            var keyStr = (string)_cache.Get(CacheKey);
            if (string.IsNullOrEmpty(keyStr))
            {
                // Récupérer le certificat depuis Azure Key Vault                
                var certificate = await GetCertificateFromKeyVaultAsync();
                if (certificate == null)
                {
                    throw new InvalidOperationException("Impossible de récupérer le certificat");    
                }

                var key = certificate.GetRSAPrivateKey();
                CacheItemPolicy policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddHours(10)
                };

                keyStr = key.ToXmlString(true);
                _cache.Add(CacheKey, keyStr, policy);
            }

            return keyStr; // Vuln: secret stored as string in memory
        }

        public async Task<X509Certificate2> GetCertificateFromKeyVaultAsync()
        {
            try
            {
                // Créer le client d'authentification selon la méthode choisie
                var credential = new ClientSecretCredential(_config.TenantId, _config.ClientId, _config.ClientSecret);

                // Créer le client Key Vault
                var client = new CertificateClient(new Uri(_config.KeyVaultUrl), credential);

                // Récupérer le certificat
                var certificateResponse = await client.DownloadCertificateAsync(_config.CertificateName);
                return certificateResponse.Value;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erreur lors de la récupération du certificat : {ex.Message}", ex);
            }
        }
    }
}
