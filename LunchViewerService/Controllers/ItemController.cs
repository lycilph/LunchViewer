using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.WindowsAzure.Mobile.Service;
using LunchViewerService.DataObjects;
using LunchViewerService.Models;

namespace LunchViewerService.Controllers
{
    public class ItemController : TableController<Item>
    {
        protected override void Initialize(HttpControllerContext controller_context)
        {
            base.Initialize(controller_context);
            var context = new LunchViewerContext();
            DomainManager = new EntityDomainManager<Item>(context, Request, Services);
        }

        // GET tables/Item
        public IQueryable<Item> GetAllItem()
        {
            return Query(); 
        }

        // GET tables/Item/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Item> GetItem(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Item/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Item> PatchItem(string id, Delta<Item> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/Item/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public async Task<IHttpActionResult> PostItem(Item item)
        {
            Item current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Item/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteItem(string id)
        {
             return DeleteAsync(id);
        }

    }
}