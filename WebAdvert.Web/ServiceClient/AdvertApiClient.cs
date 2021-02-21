using AdvertApi.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebAdvert.Web.ServiceClient
{
    public class AdvertApiClient : IAdvertApiClient
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        public AdvertApiClient(IConfiguration configuration, HttpClient httpClient)
        {
            this._configuration = configuration;
            this._httpClient = httpClient;
            var baseURL = _configuration.GetSection(key: "AdvertApi").GetValue<string>(key: "BaseUrl");
            _httpClient.BaseAddress = new Uri(baseURL);
            _httpClient.DefaultRequestHeaders.Add(name: "Content-type", value: "application/json");
        }

        public async Task<bool> Confirm(ConfimAdvertModel model)
        {
             var jsonModel = JsonConvert.SerializeObject(model);
            var response = await _httpClient.PutAsync(requestUri:new Uri(uriString:$"{_httpClient.BaseAddress}/confirm"), content: new StringContent(jsonModel))
                                                      .ConfigureAwait(continueOnCapturedContext: false);
            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }

        public async Task<AdvertResponse> Create(CreateAdvertModel model)
        {

            var jsonModel = JsonConvert.SerializeObject(model);
            var response = await _httpClient.PostAsync(requestUri:new Uri(uriString: $"{_httpClient.BaseAddress}/create"), content: new StringContent(jsonModel))
                                                      .ConfigureAwait(continueOnCapturedContext: false);
            var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
            var createAdvertResponse = JsonConvert.DeserializeObject<AdvertResponse>(responseJson);
            return createAdvertResponse;
        }
    }
}
