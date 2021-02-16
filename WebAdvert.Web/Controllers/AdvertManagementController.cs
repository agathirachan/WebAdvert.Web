using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebAdvert.Web.Models.AdvertManagement;
using WebAdvert.Web.Services;

namespace WebAdvert.Web.Controllers
{
    public class AdvertManagementController : Controller
    {
        public readonly IFileUploader _fileUploader;

        public AdvertManagementController(IFileUploader fileUploader)
        {
            this._fileUploader = fileUploader;
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
                var id = "11111";
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
                        return RedirectToAction("Inxex", controllerName: "Home");
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
            return View();
        }
    }
}
