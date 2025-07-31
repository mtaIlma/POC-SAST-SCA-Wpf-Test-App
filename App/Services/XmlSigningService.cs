using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using TestWpfApplication.Models;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Runtime.Caching;
using Wpf_Test_App.Services;

namespace TestWpfApplication.Services
{
    /// <summary>
    /// Singing service.
    /// </summary>
    public class XmlSigningService
    {
        KeyVaultService _keyVaultService;
        private AzureKeyVaultConfig _config;

        public XmlSigningService(AzureKeyVaultConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            _keyVaultService = new KeyVaultService(config);
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

                var keyStr = await _keyVaultService.GetKey();                           
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
                var certificate = await _keyVaultService.GetCertificateFromKeyVaultAsync();
                return certificate != null;
            }
            catch
            {
                return false;
            }
        }
    }
}