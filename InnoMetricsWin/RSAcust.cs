using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Security;
using System.Runtime.InteropServices;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace ConsoleApp1
{
    class RSAcust
    {

        public string RSA_Decrypt(string encryptedText, string privateKey)
        {
            CspParameters cspParams = new CspParameters { ProviderType = 1 };
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams);

            rsaProvider.ImportCspBlob(Convert.FromBase64String(privateKey));

            var buffer = Convert.FromBase64String(encryptedText);

            byte[] plainBytes = rsaProvider.Decrypt(buffer, false);
            
            string plainText = Encoding.UTF8.GetString(plainBytes, 0, plainBytes.Length);

            return plainText;
        }

        public byte[] RSA_Encrypt(byte[] data, string pathKey)
        {
            CspParameters cspParams = new CspParameters { ProviderType = 1 };
            // RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams);
            RSACryptoServiceProvider rsaProvider = GetRSAProviderFromPemFile(pathKey);
            // byte [] key = rsaProvider.ExportCspBlob(true);
           // rsaProvider.ImportCspBlob(System.Text.Encoding.UTF8.GetBytes(publicKey));

            byte[] plainBytes = data;
            byte[] encryptedBytes = rsaProvider.Encrypt(plainBytes, true);

            return encryptedBytes;
        }

        public static RSACryptoServiceProvider GetRSAProviderFromPem(String pemstr)
        {
            CspParameters cspParameters = new CspParameters();
            cspParameters.KeyContainerName = "MyKeyContainer";
            RSACryptoServiceProvider rsaKey = new RSACryptoServiceProvider(cspParameters);

            Func<RSACryptoServiceProvider, RsaKeyParameters, RSACryptoServiceProvider> MakePublicRCSP = (RSACryptoServiceProvider rcsp, RsaKeyParameters rkp) =>
            {
                RSAParameters rsaParameters = DotNetUtilities.ToRSAParameters(rkp);
                rcsp.ImportParameters(rsaParameters);
                return rsaKey;
            };

            Func<RSACryptoServiceProvider, RsaPrivateCrtKeyParameters, RSACryptoServiceProvider> MakePrivateRCSP = (RSACryptoServiceProvider rcsp, RsaPrivateCrtKeyParameters rkp) =>
            {
                RSAParameters rsaParameters = DotNetUtilities.ToRSAParameters(rkp);
                rcsp.ImportParameters(rsaParameters);
                return rsaKey;
            };

            PemReader reader = new PemReader(new StringReader(pemstr));  
            object kp = reader.ReadObject();

            // If object has Private/Public property, we have a Private PEM
            return (kp.GetType().GetProperty("Private") != null) ? MakePrivateRCSP(rsaKey, (RsaPrivateCrtKeyParameters)(((AsymmetricCipherKeyPair)kp).Private)) : MakePublicRCSP(rsaKey, (RsaKeyParameters)kp);
        }

        public static RSACryptoServiceProvider GetRSAProviderFromPemFile(String pemfile)
        {
            return GetRSAProviderFromPem(File.ReadAllText(pemfile).Trim());
        }
    }
}
