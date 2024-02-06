using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace WindowsAgent.Checks
{
    using Item = Dictionary<string, object>;

    internal class Certificate : Check
    {
        private const int _defaultInterval = 240;  // Interval in minutes, can be overwritten with REG key.
        private const string _key = "certificate";  // Check key.

        public override string Key() { return _key; }
        public override int DefaultInterval() { return _defaultInterval; }
        public override bool CanRun() { return true; }

        public override CheckResult Run()
        {
            var data = new CheckResult();
            List<Item> items = new List<Item>();

            foreach (StoreLocation storeLocation in (StoreLocation[]) Enum.GetValues(typeof(StoreLocation)))
            {
                foreach (StoreName storeName in (StoreName[]) Enum.GetValues(typeof(StoreName)))
                {
                    X509Store store = new X509Store(storeName, storeLocation);

                    try
                    {
                        store.Open(OpenFlags.OpenExistingOnly);
                    }
                    catch (CryptographicException)
                    {
                        if (Config.IsDebug())
                        {
                            Logger.Write(string.Format("Failed retrieve certifcates from store: {0} location: {1}", storeName, storeLocation), EventLogEntryType.Warning, EventId.None);
                        }
                    }

                    foreach (X509Certificate2 certificate in store.Certificates)
                    {
                        X509Chain ch = new X509Chain();
                        ch.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                        ch.Build(certificate);

                        Item item = new Item
                        {
                            ["name"] = certificate.FriendlyName,  // TODO
                            ["issuer"] = certificate.Issuer,
                            ["subject"] = certificate.Subject,
                            ["signatureAlgorithm"] = certificate.SignatureAlgorithm.FriendlyName,
                            ["version"] = certificate.Version,
                            ["isValid"] = certificate.Verify(),
                            ["validNotAfter"] = (int) certificate.NotAfter.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                            ["validNotBefore"] = (int) certificate.NotBefore.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                            ["expiresIn"] = (int) DateTime.Now.Subtract(certificate.NotAfter).TotalSeconds,
                            ["publicKeyType"] = certificate.PublicKey.Oid.FriendlyName,
                            ["publicKeyLength"] = certificate.PublicKey.EncodedKeyValue.RawData.Length,
                            ["thumbPrint"] = certificate.Thumbprint,
                            ["chainRevocationFlag"] = ch.ChainPolicy.RevocationFlag,
                            ["chainRevocationMode"] = ch.ChainPolicy.RevocationMode,
                            ["chainVerificationFlags"] = ch.ChainPolicy.VerificationFlags,
                            ["chainVerificationTime"] = ch.ChainPolicy.VerificationTime,
                            ["chainStatusLength"] = ch.ChainStatus.Length,
                            ["chainApplicationPolicyCount"] = ch.ChainPolicy.ApplicationPolicy.Count,
                            ["chainCertificatePolicyCount"] = ch.ChainPolicy.CertificatePolicy.Count,
                            ["chainElements"] = ch.ChainElements.Select(e => e.Certificate.Verify()),
                            ["chainElementsIsSynchronized"] = ch.ChainElements.IsSynchronized,
                        };

                        items.Add(item);
                    }

                    store.Close();
                }
            }

            data.AddType("certificate", items.ToArray());

            return data;
        }
    }
}
