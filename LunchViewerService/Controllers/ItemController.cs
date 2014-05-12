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
    public class ItemController : TableController<ItemEntity>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            ControllerHelper.InitializeConnectionString("StorageConnectionString", Services);
            DomainManager = new StorageDomainManager<ItemEntity>("StorageConnectionString", "Items", Request, Services);
        }

        // GET tables/ItemEntity
        public Task<IEnumerable<ItemEntity>> GetAllItemEntities()
        {
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<ItemEntity>("Items");

            var opts = new ODataQueryOptions(new ODataQueryContext(modelBuilder.GetEdmModel(), typeof(ItemEntity)), Request);
            return DomainManager.QueryAsync(opts);
        }

        // GET tables/ItemEntity
        public SingleResult<ItemEntity> GetItemEntity(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/ItemEntity
        public Task<ItemEntity> PatchItemEntity(string id, Delta<ItemEntity> patch)
        {
            return UpdateAsync(id, patch);
        }

        // POST tables/ItemEntity
        public async Task<IHttpActionResult> PostItemEntity(ItemEntity item)
        {
            var current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/ItemEntity
        public Task DeleteItemEntity(string id)
        {
            return DeleteAsync(id);
        }
    }
}