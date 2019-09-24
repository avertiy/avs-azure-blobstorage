using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using BlobStorageWebApp.Helpers;
using BlobStorageWebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BlobStorageWebApp.Controllers
{
    [Route("v1")]
    public class TestUserFlowController : Controller
    {
        private readonly AzureStorageClient _storageClient;
        private readonly string _targetApiUrl;

        public TestUserFlowController(IOptions<WebAppConfig> config)
        {
            config.Value.Validate();
            _storageClient = new AzureStorageClient(config.Value);
            _targetApiUrl = config.Value.TargetApi;
        }

        public IActionResult Index()
        {
            ViewData["TargetApiUrl"] = _targetApiUrl;
            return View();
        }

        [HttpGet]
        [Route("deposit/{userId}/documents")]
        public async Task<IActionResult> GetDocumentsList(string userId)
        {
            try
            {
                var urls = await _storageClient.GetDepositDocumentBlobNames(userId);
                return new ObjectResult(urls);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("deposit/{userId}/getdocument/{blobname}")]
        public async Task<IActionResult> GetDocument(string userId, string blobname)
        {
            try
            {
                var bytes = await _storageClient.DownloadDepositDocumentAsync(userId, blobname);
                return new FileContentResult(bytes, "image/jpeg");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("deposit/{userId}/document")]
        public async Task<IActionResult> AttachDocument(string userId, IFormFile file)
        {
            if (string.IsNullOrEmpty(userId) || file == null)
                return BadRequest();

            var result = await _storageClient.AttachDepositDocumentAsync(userId, file);
            return Ok();
        }
        
        [HttpGet]
        [Route("deposit/user")]
        public ActionResult GetUserProfile(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest();

            var model = new DepositUserResponse
            {
                FirstName = "Leeloo",
                LastName = "Dallas",
                DOB = DateTime.Today.ToShortDateString(),
                SSNRecorded = true
            };

            return new ObjectResult(model);
        }

        [HttpPost]
        [Route("deposit/{userId}")]
        public ActionResult InitiateDeposit(string userId, decimal amount, string SSN = "")
        {
            var success = new {success = true, data = new {paymentId = Guid.NewGuid()}};
            var rejected = new {success = false,message= "Deposit Limit Exceeded",data = new { depositLimit = 120, totalPending =10}};
            return new JsonResult(DateTime.Now.Second % 2 == 0 ? (object) success : rejected);
        }

        [HttpPost]
        [Route("deposit/{userId}/{paymentId}")]
        public ActionResult CompleteDeposit(string userId, string paymentId)
        {
            return Ok();
        }

        //[HttpPost]
        [Route("deposit/CompleteDeposit2/{userId}/{paymentId}")]
        public ActionResult CompleteDeposit2(string userId, string paymentId)
        {
            return new JsonResult(new { success = true });
        }

        [HttpGet]
        [Route("deposit-summary")]
        public ActionResult GetSummary(string date)
        {
            return new JsonResult(new
            {
                paymentId = "0331d4f7-1028-4eea-a8ea-42d9ccd825c2",
                firstName = "Leeloo",
                lastName = "Dallas",
                amount = "100.50",
                status = "Approved",
                updatedAt = "2018-12-12 12:12:12.123T",
            });
        }
    }
}





