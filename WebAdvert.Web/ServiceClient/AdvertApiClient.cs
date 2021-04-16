using AdvertApi.Models;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebAdvert.Web.Models.AdvertManagement;

namespace WebAdvert.Web.ServiceClient
{
    public class AdvertApiClient : IAdvertApiClient
    {
        private readonly string _baseAddress;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        public AdvertApiClient(IConfiguration configuration, HttpClient httpClient, IMapper mapper)
        {
            this._configuration = configuration;
            this._httpClient = httpClient;
            var baseURL = _configuration.GetSection(key: "AdvertApi").GetValue<string>(key: "BaseUrl");
            _httpClient.BaseAddress = new Uri(baseURL);
            _httpClient.DefaultRequestHeaders.Add(name: "Content-type", value: "application/json");
            _baseAddress = configuration.GetSection("AdvertApi").GetValue<string>("BaseUrl");
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
        public async Task<List<Advertisement>> GetAllAsync()
        {
            var apiCallResponse = await _httpClient.GetAsync(new Uri($"{_baseAddress}/all")).ConfigureAwait(false);
            var allAdvertModels =  apiCallResponse.Content.ReadAsStringAsync().Result;
            if(apiCallResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception("Error occured");
            }
            return JsonConvert.DeserializeObject<List<Advertisement>>(allAdvertModels);
            //var allAdvertModels = await apiCallResponse.Content.ReadAsAsync<List<AdvertModel>>().ConfigureAwait(false);
            // return allAdvertModels.Select(x => _mapper.Map<Advertisement>(x)).ToList();
        }

        public async Task<Advertisement> GetAsync(string advertId)
        {
            var apiCallResponse = await _httpClient.GetAsync(new Uri($"{_baseAddress}/{advertId}")).ConfigureAwait(false);
            if (apiCallResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception("Error occured");
            }
            var allAdvertModel = apiCallResponse.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<Advertisement>(allAdvertModel);
            //var fullAdvert = await apiCallResponse.Content.ReadAsAsync<AdvertModel>().ConfigureAwait(false);
            //return _mapper.Map<Advertisement>(fullAdvert);
        }
    }
}
