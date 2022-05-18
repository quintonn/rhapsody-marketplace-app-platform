using Marketplace.Models;
using Marketplace.SiteSpecific;
using NHibernate;
using System.Linq;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems.CoreItems;

namespace Marketplace.BackEnd.MarketItems
{
    public class DeleteMarketplaceItem : CoreDeleteAction<UserItem>
    {
        public DeleteMarketplaceItem(DataService dataService) : base(dataService)
        {
        }

        public override string EntityName => "Marketplace Item";

        public override EventNumber ViewNumber => MenuNumber.ViewMarketplaceItems;

        public override EventNumber GetId()
        {
            return MenuNumber.DeleteMarketplaceItem;
        }

        public override void DeleteOtherItems(ISession session, UserItem mainItem)
        {
            var dbFiles = session.QueryOver<FileItem>().Where(f => f.UserItem.Id == mainItem.Id).List().ToList();
            foreach (var item in dbFiles)
            {
                DataService.TryDelete(session, item);
            }
            base.DeleteOtherItems(session, mainItem);
        }
    }
}
