using QBic.Core.Data.BaseTypes;
using QBic.Core.Models;
using System;
using WebsiteTemplate.Models;

namespace Marketplace.Models
{
    public class UserItem : DynamicClass
    {
        public virtual string Name { get; set; }
        public virtual LongString Description { get; set; }
        public virtual LongString Details { get; set; }
        public virtual User Owner { get; set; }        
        public virtual DateTime LastUpdate { get; set; }
    }
}
