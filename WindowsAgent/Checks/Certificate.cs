using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace WindowsAgent.Checks
{
    using Item = Dictionary<string, object>;

    internal class Certificate : Check
    {
        private const int _defaultInterval = 5;  // Interval in minutes, can be overwritten with REG key.
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
                        Item item = new Item
                        {
                            ["name"] = certificate.Subject,
                            ["issuer"] = certificate.Issuer,
                            ["subject"] = certificate.Subject,
                            ["signatureAlgorithm"] = certificate.SignatureAlgorithm.FriendlyName,
                            ["hash"] = certificate.GetPublicKey(),
                            ["validNotAfter"] = (int) certificate.NotAfter.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                            ["validNotBefore"] = (int) certificate.NotBefore.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                            ["expiresIn"] = (int) DateTime.Now.Subtract(certificate.NotAfter).TotalSeconds,
                        };

                        // TODO chain elements als apart type?
                        // X509Chain ch = new X509Chain();
                        // ch.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                        // ch.Build(certificate);

                        // Console.WriteLine ("Chain Information");
                        // Console.WriteLine ("Chain revocation flag: {0}", ch.ChainPolicy.RevocationFlag);
                        // Console.WriteLine ("Chain revocation mode: {0}", ch.ChainPolicy.RevocationMode);
                        // Console.WriteLine ("Chain verification flag: {0}", ch.ChainPolicy.VerificationFlags);
                        // Console.WriteLine ("Chain verification time: {0}", ch.ChainPolicy.VerificationTime);
                        // Console.WriteLine ("Chain status length: {0}", ch.ChainStatus.Length);
                        // Console.WriteLine ("Chain application policy count: {0}", ch.ChainPolicy.ApplicationPolicy.Count);
                        // Console.WriteLine ("Chain certificate policy count: {0} {1}", ch.ChainPolicy.CertificatePolicy.Count, Environment.NewLine);

                        // //Output chain element information.
                        // Console.WriteLine ("Chain Element Information");
                        // Console.WriteLine ("Number of chain elements: {0}", ch.ChainElements.Count);
                        // Console.WriteLine ("Chain elements synchronized? {0} {1}", ch.ChainElements.IsSynchronized, Environment.NewLine);

                        // foreach (X509ChainElement element in ch.ChainElements)
                        // {
                        //     Console.WriteLine ("Element issuer name: {0}", element.Certificate.Issuer);
                        //     Console.WriteLine ("Element certificate valid until: {0}", element.Certificate.NotAfter);
                        //     Console.WriteLine ("Element certificate is valid: {0}", element.Certificate.Verify ());
                        //     Console.WriteLine ("Element error status length: {0}", element.ChainElementStatus.Length);
                        //     Console.WriteLine ("Element information: {0}", element.Information);
                        //     Console.WriteLine ("Number of element extensions: {0}{1}", element.Certificate.Extensions.Count, Environment.NewLine);

                        //     if (ch.ChainStatus.Length > 1)
                        //     {
                        //         for (int index = 0; index < element.ChainElementStatus.Length; index++)
                        //         {
                        //             Console.WriteLine (element.ChainElementStatus[index].Status);
                        //             Console.WriteLine (element.ChainElementStatus[index].StatusInformation);
                        //         }
                        //     }
                        // }

                        items.Add(item);
                        break;
                    }

                    store.Close();
                }
            }

            data.AddType("certificate", items.ToArray());

            return data;
        }
    }
}
