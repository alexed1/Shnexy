using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using KwasantCore.Managers.APIManagers.Authorizers;

namespace KwasantCore.Managers.APIManagers.Transmitters.Http
{
    public class OAuthHttpChannel : IAuthorizingHttpChannel
    {
        private readonly IOAuthAuthorizer _authorizer;

        public OAuthHttpChannel(IOAuthAuthorizer authorizer)
        {
            _authorizer = authorizer;
        }

        /// <summary>
        /// Http request may be sent several times in the scope of this method but some .NET http clients don't support sending the same request instance twice.
        /// </summary>
        /// <param name="requestFactoryMethod"></param>
        /// <param name="userId">User ID for authorization</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SendRequestAsync(Func<HttpRequestMessage> requestFactoryMethod, string userId)
        {
            HttpResponseMessage response;
            using (var client = new HttpClient())
            {
                do
                {
                    using (var request = requestFactoryMethod())
                    {
                        // after request is created access token is added as authorization header.
                        var accessToken = await _authorizer.GetAccessTokenAsync(userId, CancellationToken.None);
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                        response = await client.SendAsync(request);
                        if (!response.IsSuccessStatusCode)
                        {
                            if (response.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                // if 401 got try to refresh access token and send request again
                                await _authorizer.RefreshTokenAsync(userId, CancellationToken.None);
                                response.Dispose();
                            }
                            else
                            {
                                response.EnsureSuccessStatusCode();
                            }
                        }
                    }
                } while (!response.IsSuccessStatusCode);
            }
            return response;
        }
    }
}