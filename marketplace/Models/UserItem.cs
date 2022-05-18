using QBic.Core.Data.BaseTypes;
using QBic.Core.Models;
using System;

namespace Marketplace.Models
{
    public class UserItem : DynamicClass
    {
        public virtual string Name { get; set; }
        public virtual LongString Description { get; set; }
        public virtual LongString Details { get; set; }
        public virtual string OwnerId { get; set; }

        public virtual DateTime LastUpdate { get; set; }
    }
}
