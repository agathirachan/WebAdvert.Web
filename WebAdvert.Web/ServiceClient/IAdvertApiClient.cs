using AdvertApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAdvert.Web.Models.AdvertManagement;

namespace WebAdvert.Web.ServiceClient
{
    public interface IAdvertApiClient
    {
        Task<AdvertResponse> Create(CreateAdvertModel model);
        Task<bool> Confirm(ConfimAdvertModel model);
        Task<List<Advertisement>> GetAllAsync();
        Task<Advertisement> GetAsync(string advertId);
    }
}
