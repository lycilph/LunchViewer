using Microsoft.WindowsAzure.Mobile.Service;
using Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Query;

namespace lunchviewerService.Controllers
{
    public class MenuController : TableController<MenuEntity>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            ControllerHelper.InitializeConnectionString("StorageConnectionString", Services);
            DomainManager = new StorageDomainManager<MenuEntity>("StorageConnectionString", "Menus", Request, Services);
        }

        // GET tables/MenuEntity
        public Task<IEnumerable<MenuEntity>> GetAllMenuEntities()
        {
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<MenuEntity>("Menus");

            var opts = new ODataQueryOptions(new ODataQueryContext(modelBuilder.GetEdmModel(), typeof(MenuEntity)), Request);
            return DomainManager.QueryAsync(opts);
        }

        // GET tables/MenuEntity
        public SingleResult<MenuEntity> GetMenuEntity(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/MenuEntity
        public Task<MenuEntity> PatchMenuEntity(string id, Delta<MenuEntity> patch)
        {
            return UpdateAsync(id, patch);
        }

        // POST tables/MenuEntity
        public async Task<IHttpActionResult> PostMenuEntity(MenuEntity item)
        {
            var current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/MenuEntity
        public Task DeleteMenuEntity(string id)
        {
            return DeleteAsync(id);
        }
    }
}