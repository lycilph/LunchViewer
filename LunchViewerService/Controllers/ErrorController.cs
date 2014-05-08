using Microsoft.WindowsAzure.Mobile.Service;
using ModelLibrary;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Query;

namespace lunchviewerService.Controllers
{
    public class ErrorController : TableController<ErrorEntity>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            DomainManager = new StorageDomainManager<ErrorEntity>("StorageConnectionString", "Errors", Request, Services);
        }

        // GET tables/ErrorEntity
        public Task<IEnumerable<ErrorEntity>> GetAllErrorEntities()
        {
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<ErrorEntity>("Errors");

            var opts = new ODataQueryOptions(new ODataQueryContext(modelBuilder.GetEdmModel(), typeof(ErrorEntity)), Request);
            return DomainManager.QueryAsync(opts);
        }

        // GET tables/ErrorEntity
        public SingleResult<ErrorEntity> GetErrorEntity(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/ErrorEntity
        public Task<ErrorEntity> PatchErrorEntity(string id, Delta<ErrorEntity> patch)
        {
            return UpdateAsync(id, patch);
        }

        // POST tables/ErrorEntity
        public async Task<IHttpActionResult> PostErrorEntity(ErrorEntity item)
        {
            var current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/ErrorEntity
        public Task DeleteErrorEntity(string id)
        {
            return DeleteAsync(id);
        }
    }
}