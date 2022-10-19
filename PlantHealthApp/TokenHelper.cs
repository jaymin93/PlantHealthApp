using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AuthenticationResult = Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationResult;
using ClientCredential = Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential;

namespace PlantHealthApp
{
    public static class TokenHelper
    {
        public static async Task<string> GetAccessTokenAsync(string authority, string resource, string scope)
        {
            var context = new AuthenticationContext(authority);
            ClientCredential credential = new ClientCredential("7477378a-94f8-4158-9c6e-fe5d776cc71a", "9OY8Q~Q21dj1s1A08lAcC6ITo2GBTsyTO3TAZaar");
            AuthenticationResult result = await context.AcquireTokenAsync(resource, credential);
            Trace.WriteLine(result.AccessToken);
            if (result == null)
                throw new InvalidOperationException("Failed to obtain the JWT token");

            return result.AccessToken;
        }
    }
}
