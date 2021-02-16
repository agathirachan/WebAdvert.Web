using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebAdvert.Web.Services
{
    public class S3FileUploader : IFileUploader
    {
        private readonly IConfiguration _configuration;
        public S3FileUploader(IConfiguration configuration)
        {
            this._configuration = configuration;
        }
        public async Task<bool> UploadFileAsync(string fileName, Stream storageStream)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException(message: "File name must be specified.");
            var bucketName = _configuration.GetValue<string>(key: "ImageBucket");

            using (var client = new AmazonS3Client())
            {
                if(storageStream.Length> 0)
                {
                    if (storageStream.CanSeek)
                        storageStream.Seek(offset: 0, SeekOrigin.Begin);
                }

                var request = new PutObjectRequest
                {
                    AutoCloseStream = true,
                    BucketName = bucketName,
                    InputStream = storageStream,
                    Key = fileName
                };
                var response = await client.PutObjectAsync(request).ConfigureAwait(continueOnCapturedContext: false);
                return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }

        }
    }
}
