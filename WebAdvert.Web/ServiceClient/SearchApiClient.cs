
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WebAdvert.Web.Models.AdvertManagement;

namespace WebAdvert.Web.ServiceClient
{
    public class SearchApiClient : ISearchApiClient
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        private readonly string BaseAddress = string.Empty;
        public SearchApiClient(HttpClient client, IConfiguration configuration)
        {
            _client = client;
            BaseAddress = configuration.GetSection(key: "SearchApi").GetValue<string>(key: "url");
        }

        public async Task<List<AdvertType>> Search(string keyword)
        {
            var result = new List<AdvertType>();
            var callUrl = $"{BaseAddress}/search/v1/{keyword}";
            var httpResponse = await _client.GetAsync(new Uri(callUrl)).ConfigureAwait(false);

            if (httpResponse.StatusCode == HttpStatusCode.OK)
            {
                // var allAdverts = await httpResponse.Content.ReadAsAsync<List<AdvertType>>().ConfigureAwait(false);
                //result.AddRange(allAdverts);
                var allAdverts = httpResponse.Content.ReadAsStringAsync().Result;
                result = JsonConvert.DeserializeObject<List<AdvertType>>(allAdverts);
               
            }

            return result;
        }
    }
}
