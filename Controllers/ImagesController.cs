using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlobStorageWebApp.Models;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.AspNetCore.Http;
using BlobStorageWebApp.Helpers;

namespace BlobStorageWebApp.Controllers
{
    [Route("api/[controller]")]
    public class ImagesController : Controller
    {
        private readonly AzureStorageClient _storageClient;

        public ImagesController(IOptions<AzureStorageConfig> config)
        {
            //as this is a demo app i call setup SetupContainers in constructor for simplicity
            _storageClient = new AzureStorageClient(config.Value);
        }

        // POST /api/images/upload
        [HttpPost("[action]")]
        public async Task<IActionResult> Upload(ICollection<IFormFile> files)
        {
            bool isUploaded = false;

            try
            {

                if (files.Count == 0)
                    return BadRequest("No files received from the upload");

                foreach (var formFile in files)
                {
                    if (StorageHelper.IsImage(formFile) && formFile.Length > 0)
                    {
                        isUploaded = await _storageClient.AttachDepositDocument("user#123", formFile);
                    }
                    else
                    {
                        return new UnsupportedMediaTypeResult();
                    }
                }

                if (isUploaded)
                {
                    return new AcceptedAtActionResult("GetThumbNails", "Images", null, null);
                }
                else
                    return BadRequest("Look like the image couldnt upload to the storage");
            }
            catch (StorageException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET /api/images/thumbnails
        [HttpGet("thumbnails")]
        public async Task<IActionResult> GetThumbNails()
        {
            try
            {
                List<string> thumbnailUrls = await _storageClient.GetDepositDocumentUrls("user#123");
                return new ObjectResult(thumbnailUrls);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}