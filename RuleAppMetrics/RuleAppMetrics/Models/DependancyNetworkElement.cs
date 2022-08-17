using InRule.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InRuleLabs.AuthoringExtensions.RuleAppMetrics.Models
{
    internal class DependancyNetworkElement
    {
        public Guid Id;
        public string Name;
        public List<DependancyNetworkElement> ConsumedBy;
        public List<DependancyNetworkElement> UpdatedBy;
        public List<DependancyNetworkElement> FiredBy;

        public DependancyNetworkElement(RuleRepositoryDefBase element)
        {
            Name = element.Name;
            Id = element.Guid;
            ConsumedBy = new List<DependancyNetworkElement>();
            UpdatedBy = new List<DependancyNetworkElement>();
            FiredBy = new List<DependancyNetworkElement>();
        }
    }
}
