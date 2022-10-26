using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Diagnostics;

namespace PlantHealthConsoleApp
{
    public static class TokenHelper
    {
        public static string clientID;
        public static string clientSecret;
        public static async Task<string> GetAccessTokenAsync(string authority, string resource, string scope)
        {
            var context = new AuthenticationContext(authority);
            ClientCredential credential = new ClientCredential(clientID,clientSecret);
            AuthenticationResult result = await context.AcquireTokenAsync(resource, credential);
            Trace.WriteLine(result.AccessToken);
            if (result == null)
                throw new InvalidOperationException("Failed to obtain the JWT token");

            return result.AccessToken;
        }
    }
}
