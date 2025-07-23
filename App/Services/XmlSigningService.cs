using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using TestWpfApplication.Models;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Runtime.Caching;

namespace TestWpfApplication.Services
{
    public class XmlSigningService
    {
        private AzureKeyVaultConfig _config;
        private static readonly MemoryCache _cache = MemoryCache.Default;
        private const string CacheKey = "MyRsaKey";

        public XmlSigningService(AzureKeyVaultConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<SigningResult> SignXmlFileAsync(string xmlFilePath, string outputPath = null)
        {
            var result = new SigningResult();

            try
            {
                // Validation des paramètres
                if (!File.Exists(xmlFilePath))
                {
                    result.Message = "Le fichier XML spécifié n'existe pas.";
                    return result;
                }

                if (!_config.IsValid())
                {
                    result.Message = "Configuration Azure Key Vault invalide.";
                    return result;
                }

                // Définir le chemin de sortie
                outputPath ??= Path.Combine(
                    Path.GetDirectoryName(xmlFilePath),
                    $"{Path.GetFileNameWithoutExtension(xmlFilePath)}_signed{Path.GetExtension(xmlFilePath)}"
                );


                var keyStr = (string)_cache.Get(CacheKey);
                if (string.IsNullOrEmpty(keyStr))               
                {
                    // Récupérer le certificat depuis Azure Key Vault
                    result.Message = "Récupération du certificat depuis Azure Key Vault...";
                    var certificate = await GetCertificateFromKeyVaultAsync();
                    if (certificate == null)
                    {
                        result.Message = "Impossible de récupérer le certificat depuis Azure Key Vault.";
                        return result;
                    }

                    var key = certificate.GetRSAPrivateKey();
                    CacheItemPolicy policy = new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTimeOffset.Now.AddHours(10)
                    };

                    keyStr = key.ToXmlString(true);
                    _cache.Add(CacheKey, keyStr, policy);
                }

                RSACryptoServiceProvider rsaKey = new RSACryptoServiceProvider();
                rsaKey.FromXmlString(keyStr);


                // Charger le document XML
                result.Message = "Chargement du document XML...";
                var xmlDoc = new XmlDocument { PreserveWhitespace = true };
                xmlDoc.Load(xmlFilePath);

                // Signer le document
                result.Message = "Signature du document XML en cours...";
                SignXmlDocument(xmlDoc, rsaKey);

                // Sauvegarder le document signé
                result.Message = "Sauvegarde du document signé...";
                xmlDoc.Save(outputPath);

                result.Success = true;
                result.SignedFilePath = outputPath;
                result.Message = "Document XML signé avec succès.";

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Erreur lors de la signature : {ex.Message}";
                result.Exception = ex;
                return result;
            }
        }

        private async Task<X509Certificate2> GetCertificateFromKeyVaultAsync()
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

        private void SignXmlDocument(XmlDocument xmlDoc, RSA key)
        {
            
            // Créer un objet SignedXml
            var signedXml = new SignedXml(xmlDoc);
           
            signedXml.SigningKey = key;

            // Créer une référence à l'ensemble du document
            var reference = new Reference("");
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            signedXml.AddReference(reference);

               
            // Calculer la signature
            signedXml.ComputeSignature();

            // Obtenir l'élément de signature XML
            var xmlSignature = signedXml.GetXml();

            // Ajouter la signature au document
            xmlDoc.DocumentElement?.AppendChild(xmlDoc.ImportNode(xmlSignature, true));
        }

        public bool VerifyXmlSignature(string signedXmlFilePath)
        {
            try
            {
                var xmlDoc = new XmlDocument { PreserveWhitespace = true };
                xmlDoc.Load(signedXmlFilePath);

                // Rechercher l'élément de signature
                var signatureNode = xmlDoc.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#")[0];

                if (signatureNode == null)
                    return false;

                var signedXml = new SignedXml(xmlDoc);
                signedXml.LoadXml((XmlElement)signatureNode);

                // Vérifier la signature
                return signedXml.CheckSignature();
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var certificate = await GetCertificateFromKeyVaultAsync();
                return certificate != null;
            }
            catch
            {
                return false;
            }
        }
    }
}