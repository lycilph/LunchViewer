using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using LunchViewerService.Utils;
using Microsoft.WindowsAzure.Mobile.Service;
using LunchViewerService.DataObjects;
using LunchViewerService.Models;

namespace LunchViewerService.Controllers
{
    public class MenuController : TableController<Menu>
    {
        protected override void Initialize(HttpControllerContext controller_context)
        {
            base.Initialize(controller_context);
            var context = new LunchViewerContext();
            DomainManager = new EntityDomainManager<Menu>(context, Request, Services);
        }

        // GET tables/Menu
        [QueryableExpand("Items")]
        public IQueryable<Menu> GetAllMenu()
        {
            return Query();
        }

        // GET tables/Menu/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Menu> GetMenu(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Menu/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Menu> PatchMenu(string id, Delta<Menu> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/Menu/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public async Task<IHttpActionResult> PostMenu(Menu item)
        {
            Menu current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Menu/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteMenu(string id)
        {
             return DeleteAsync(id);
        }

    }
}