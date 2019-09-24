using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlobStorageWebApp.Models
{
    public class WebAppConfig
    {
        public string AccountName { get; set; }
        public string AccountKey { get; set; }
        public string IdentificationDocsContainer { get; set; }
        public string DepositDocsContainer { get; set; }
        public string TargetApi { get; set; }

        public void Validate()
        {
            if (string.IsNullOrEmpty(AccountName))
                throw new Exception("WebAppConfig=> AccountName is missing");

            if (string.IsNullOrEmpty(AccountKey))
                throw new Exception("WebAppConfig=> AccountKey is missing");

            if (string.IsNullOrEmpty(IdentificationDocsContainer))
                throw new Exception("WebAppConfig=> IdentificationDocsContainer is missing");

            if (string.IsNullOrEmpty(DepositDocsContainer))
                throw new Exception("WebAppConfig=> DepositDocsContainer is missing");
            if (string.IsNullOrEmpty(TargetApi))
                TargetApi = "http://localhost:55872/v1/";
        }
    }
}
