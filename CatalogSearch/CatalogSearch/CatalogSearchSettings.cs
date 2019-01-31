using InRule.Repository.Client;
using System;

namespace CatalogSearch
{
    public class CatalogSearchSettings
    {
        public string CatalogServiceUrl { get; set; }
        public string CatalogUsername { get; set; }
        public string CatalogPassword { get; set; }
        public Guid? CurrentRuleApp { get; set; }

        public CatalogSearchSettings(RuleCatalogConnection connection, Guid? currentRuleApp)
        {
            CatalogServiceUrl = connection.ServiceUri;
            CatalogUsername = connection.User?.Name;
            CatalogPassword = connection.User?.Password;
            CurrentRuleApp = currentRuleApp;
        }
    }
}