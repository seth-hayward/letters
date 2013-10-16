using System.Web;
using Microsoft.AspNet.SignalR;
namespace Microsoft.AspNet.SignalR
{
    public static class SystemWebExtensions
    {
        public static HttpContextBase GetHttpContext(this IRequest request)
        {
            object value;
            if (request.Environment.TryGetValue(typeof(HttpContextBase).FullName, out value))
            {
                return (HttpContextBase)value;
            }
            return null;
        }
    }
}