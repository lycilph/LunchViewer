using System;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace LunchViewerService.Utils
{
    public class QueryableExpandAttribute : ActionFilterAttribute
    {
        private const string ODataExpandOption = "$expand=";

        public string PropertyName { get; set; }

        public QueryableExpandAttribute(string property_name)
        {
            PropertyName = property_name;
        }

        public override void OnActionExecuting(HttpActionContext action_context)
        {
            var request = action_context.Request;
            var query = request.RequestUri.Query.Substring(1);
            var parts = query.Split('&').ToList();
            var found_expand = false;
            for (var i = 0; i < parts.Count; i++)
            {
                var segment = parts[i];
                if (!segment.StartsWith(ODataExpandOption, StringComparison.Ordinal)) 
                    continue;

                found_expand = true;
                parts[i] += "," + PropertyName;
            }

            if (!found_expand)
            {
                parts.Add(ODataExpandOption + PropertyName);
            }

            var modified_request_uri = new UriBuilder(request.RequestUri)
            {
                Query = string.Join("&", parts.Where(p => p.Length > 0))
            };
            request.RequestUri = modified_request_uri.Uri;
            base.OnActionExecuting(action_context);
        }
    }
}