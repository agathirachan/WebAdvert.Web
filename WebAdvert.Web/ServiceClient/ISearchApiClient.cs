using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAdvert.Web.Models.AdvertManagement;

namespace WebAdvert.Web.ServiceClient
{
    public interface ISearchApiClient
    {
        Task<List<AdvertType>> Search(string keyword);
    }
}
