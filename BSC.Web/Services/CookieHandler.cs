using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace BSC.Web.Services
{
    public class CookieHandler : DelegatingHandler
    {
        public CookieHandler()
        {
            InnerHandler = new HttpClientHandler();
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
