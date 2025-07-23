using System;

namespace TestWpfApplication.Models
{
    public class AzureKeyVaultConfig
    {
        public string KeyVaultUrl { get; set; }
        public string CertificateName { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TenantId { get; set; }
        public AuthenticationMethod AuthMethod { get; set; }

        public AzureKeyVaultConfig()
        {
            AuthMethod = AuthenticationMethod.ClientSecret;
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(KeyVaultUrl) &&
                   !string.IsNullOrWhiteSpace(CertificateName) &&
                   !string.IsNullOrWhiteSpace(TenantId) &&
                   (AuthMethod == AuthenticationMethod.ManagedIdentity ||
                    (!string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret)));
        }
    }

    public enum AuthenticationMethod
    {
        ClientSecret,
        ManagedIdentity,
        InteractiveBrowser
    }

    public class SigningResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string SignedFilePath { get; set; }
        public DateTime SigningTime { get; set; }
        public string CertificateThumbprint { get; set; }
        public Exception Exception { get; set; }

        public SigningResult()
        {
            SigningTime = DateTime.Now;
        }
    }
}