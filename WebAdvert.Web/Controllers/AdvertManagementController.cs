using AdvertApi.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebAdvert.Web.Models.AdvertManagement;
using WebAdvert.Web.ServiceClient;
using WebAdvert.Web.Services;

namespace WebAdvert.Web.Controllers
{
    public class AdvertManagementController : Controller
    {
        public readonly IFileUploader _fileUploader;
        public readonly IAdvertApiClient _advertApiClient;
        public readonly IMapper _mapper;

        public AdvertManagementController(IFileUploader fileUploader,
                                          IAdvertApiClient advertApiClient,
                                          IMapper mapper)
        {
            this._fileUploader = fileUploader;
            this._advertApiClient = advertApiClient;
            this._mapper = mapper;
        }
        [HttpGet]
        public IActionResult Create(CreateAdvertViewModel model)
        {
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAdvertViewModel model, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                // _advertApiClient.Create
                var createAdvertModel = _mapper.Map<CreateAdvertModel>(model);
                var apiCallResponse = await _advertApiClient.Create(createAdvertModel);

                var id = apiCallResponse.Id;

                var fileName = "";
                if(imageFile != null)
                {
                    fileName = !string.IsNullOrWhiteSpace(imageFile.FileName) ? Path.GetFileName(imageFile.FileName) : id;
                    var filePath = $"{id}/{fileName}";
                    try
                    {
                        using (var readStream = imageFile.OpenReadStream())
                        {
                            var result = await _fileUploader.UploadFileAsync(filePath, readStream)
                                                                 .ConfigureAwait(continueOnCapturedContext: false);
                            if (!result)
                                throw new Exception("Could not upload the image to file repository. Please see the logs for more detail.");
                        }
                        var confirmModel = new ConfimAdvertModel
                        {
                            Id = id,
                            FilePath = filePath,
                            Status = AdvertStatus.Active
                        };
                        
                        var conConfrim= await  _advertApiClient.Confirm(confirmModel);
                        if (!conConfrim)
                        {
                            throw new Exception(message: $"Cannot confirm advert of id = {id}");
                        }
                        return RedirectToAction("Inxex", controllerName: "Home");
                    }
                    catch(Exception ex)
                    {
                        var confirmModel = new ConfimAdvertModel
                        {
                            Id = id,
                            FilePath = filePath,
                            Status = AdvertStatus.Pending
                        };

                        var conConfrim = await _advertApiClient.Confirm(confirmModel);
                        Console.WriteLine(ex);
                    }
                }
            }
            return View();
        }
    }
}
