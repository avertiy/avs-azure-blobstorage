using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlobStorageWebApp.Helpers;
using BlobStorageWebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BlobStorageWebApp.Controllers
{
    [Route("api/[controller]")]
    public class StorageController : Controller
    {
        private readonly AzureStorageClient _storageClient;

        public StorageController(IOptions<WebAppConfig> config)
        {
            _storageClient = new AzureStorageClient(config.Value);
        }

        // POST /api/storage/attachdocument
        [HttpPost("[action]")]
        public async Task<ActionResult> AttachDocument(string userId, ICollection<IFormFile> files)
        {
            if (files.Count == 0)
                return BadRequest("No files received from the upload");

            if (files.Count == 0)
                return BadRequest("No files received from the upload");

            foreach (var file in files)
            {
                if (!StorageHelper.IsImage(file))
                    continue;
                var result = await _storageClient.AttachDepositDocumentAsync(userId, file);
            }
            
            return new AcceptedAtActionResult("ViewDocuments", "Storage", new { userId }, null);
        }

        [HttpGet("viewdocuments/")]
        public async Task<IActionResult> ViewDocuments(string userId)
        {
            try
            {
                List<string> thumbnailUrls = await _storageClient.GetDepositDocumentUrls(userId);
                return new ObjectResult(thumbnailUrls);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}