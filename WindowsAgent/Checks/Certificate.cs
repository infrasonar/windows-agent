using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace WindowsAgent.Checks
{
    using Item = Dictionary<string, object>;
    using Items = Dictionary<string, Dictionary<string, object>>;

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
            Items items = new Items();

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
                        Item item = new Item
                        {
                            ["name"] = certificate.Thumbprint,
                            ["Issuer"] = certificate.Issuer,
                            ["Subject"] = certificate.Subject,
                            ["Version"] = certificate.Version,
                            ["IsValid"] = certificate.Verify(),
                            ["NotAfter"] = (int) certificate.NotAfter.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                            ["NotBefore"] = (int) certificate.NotBefore.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                            ["ExpiresIn"] = (int) DateTime.Now.Subtract(certificate.NotAfter).TotalSeconds,
                            ["SignatureAlgorithm"] = certificate.SignatureAlgorithm,
                            ["PublicKeyType"] = certificate.PublicKey.Oid.FriendlyName,
                            ["PublicKeyLength"] = certificate.PublicKey.EncodedKeyValue.RawData.Length,
                            ["FriendlyName"] = certificate.FriendlyName,
                        };

                        foreach (string d in certificate.Issuer.Split(","))
                        {
                            string[] e = d.Trim().Split("=");
                            if (e[0] == "C")
                            {
                                item["Country"] = e[1];
                            }
                            if (e[0] == "CN")
                            {
                                item["CommonName"] = e[1];
                            }
                            if (e[0] == "O")
                            {
                                item["Organisation"] = e[1];
                            }
                            if (e[0] == "OU")
                            {
                                item["OrganizationalUnit"] = e[1];
                            }
                        }

                        items[certificate.Thumbprint] = item;
                    }

                    foreach (X509Certificate2 certificate in store.Certificates)
                    {
                        X509Chain ch = new X509Chain();
                        ch.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                        ch.Build(certificate);

                        Item item = items[certificate.Thumbprint];
                        item["ChainRevocationFlag"] = ch.ChainPolicy.RevocationFlag;
                        item["ChainRevocationMode"] = ch.ChainPolicy.RevocationMode;
                        item["ChainVerificationFlags"] = ch.ChainPolicy.VerificationFlags;
                        item["ChainVerificationTime"] = ch.ChainPolicy.VerificationTime;
                        item["ChainStatusLength"] = ch.ChainStatus.Length;
                        item["ChainApplicationPolicyCount"] = ch.ChainPolicy.ApplicationPolicy.Count;
                        item["ChainCertificatePolicyCount"] = ch.ChainPolicy.CertificatePolicy.Count;
                        item["ChainElementsIsSynchronized"] = ch.ChainElements.IsSynchronized;

                        for (int i=1; i < ch.ChainElements.Count; i++)
                        {
                            if (items.ContainsKey(ch.ChainElements[i].Certificate.Thumbprint))
                            {
                                items[ch.ChainElements[i].Certificate.Thumbprint]["parent"] = ch.ChainElements[i-1].Certificate.Thumbprint;
                            }
                        }
                    }

                    store.Close();
                }
            }

            data.AddType("certificate", items.Values.ToArray());

            return data;
        }
    }
}