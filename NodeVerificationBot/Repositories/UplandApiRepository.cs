using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NodeVerificationBot.Interfaces;
using NodeVerificationBot.Types;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace NodeVerificationBot.Repositories
{
    public class UplandApiRepository : IUplandApiRepository
    {
        private HttpClient httpClient;
        private HttpClient authHttpClient;
        private readonly IConfiguration _configuration;

        public UplandApiRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            this.httpClient = new HttpClient();
            this.httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            this.httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");

            this.authHttpClient = new HttpClient();
            this.authHttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            this.authHttpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
            this.authHttpClient.DefaultRequestHeaders.Add("Authorization", _configuration.GetSection("AppSettings")["UplandAuthToken"]);
        }

        public async Task<UplandProperty> GetPropertyById(long propertyId)
        {
            UplandProperty property;
            string requestUri = @"https://api.upland.me/properties/" + propertyId;

            property = await CallApi<UplandProperty>(requestUri);

            return property;
        }

        public async Task<List<UplandAuthProperty>> GetPropertysByUsername(string username)
        {
            List<UplandAuthProperty> properties;
            string requestUri = @"https://api.upland.me/properties/list/" + username;

            properties = await CallApi<List<UplandAuthProperty>>(requestUri, true);

            return properties;
        }

        private async Task<T> CallApi<T>(string requestUri, bool useAuth = false)
        {
            HttpResponseMessage httpResponse;
            string responseJson;

            if (useAuth)
            {
                httpResponse = await this.authHttpClient.GetAsync(requestUri);
            }
            else
            {
                httpResponse = await this.httpClient.GetAsync(requestUri);
            }
            responseJson = await httpResponse.Content.ReadAsStringAsync();

            try
            {
                return JsonConvert.DeserializeObject<T>(responseJson);
            }
            catch
            {
                return (T)Activator.CreateInstance(typeof(T));
            }
        }
    }
}
