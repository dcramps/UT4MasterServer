﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UT4MasterServer.Authorization;
using UT4MasterServer.Models;
using UT4MasterServer.Services;

namespace UT4MasterServer.Controllers
{
    [ApiController]
    [Route("ut/api/cloudstorage")]
    [AuthorizeBearer]
	[Produces("application/octet-stream")]
	public class CloudstorageController : JsonAPIController
	{
        private readonly ILogger<SessionController> logger;
        private readonly SessionService sessionService;
		private readonly CloudstorageService cloudstorageService;

		public CloudstorageController(SessionService sessionService, CloudstorageService cloudstorageService, ILogger<SessionController> logger)
        {
            this.sessionService = sessionService;
            this.cloudstorageService = cloudstorageService;
            this.logger = logger;
        }

        [HttpGet("user/{id}")]
        public async Task<IActionResult> ListUserfiles(string id)
        {
			// list all files this user has in storage - any user can see files from another user

			/* [
			
            {"uniqueFilename":"user_progression_1",
            "filename":"user_progression_1",
            "hash":"32a17bdf348e653a5cc7f94c3afb404301502d43",
            "hash256":"7dcfaac101dbba0337e1b51bf3c088e591742d5f1c299f10cc0c9da01eab5fe8",
            "length":21,
            "contentType":"text/plain",
            "uploaded":"2020-05-24T07:10:43.198Z",
            "storageType":"S3",
            "accountId":"64bf8c6d81004e88823d577abe157373"
            },
            ]
			
			
			*/

			var files = await cloudstorageService.ListFilesAsync(EpicID.FromString(id));

			var arr = new JArray();
			foreach (var file in files)
			{
				var obj = new JObject();
				obj.Add("uniqueFilename", file.Filename);
				obj.Add("filename", file.Filename);
				obj.Add("hash", file.Hash);
				obj.Add("hash256", file.Hash256);
				obj.Add("length", file.Length);
				obj.Add("contentType", "text/plain"); // this seems to be constant
				obj.Add("uploaded", file.UploadedAt.ToStringISO());
				obj.Add("storageType", "S3");
				obj.Add("accountId", id);
				arr.Add(obj);
			}

			return Json(arr);
        }

        [HttpGet("user/{id}/{filename}")]
        public async Task<IActionResult> GetUserfile(string id, string filename)
        {
			// get the user file from cloudstorage - any user can see files from another user

			var file = await cloudstorageService.GetFileAsync(EpicID.FromString(id), filename);
			if (file == null)
			{
				return Json(new ErrorResponse() { ErrorMessage = $"Sorry, we couldn't find a file {filename} for account {id}" }, 404);
			}

			return new FileContentResult(file.RawContent, "application/octet-stream");

			//// Temp data
			//string path = "user_profile_2.local";
			//byte[] data = await System.IO.File.ReadAllBytesAsync(path);
			//return new FileContentResult(data, "application/octet-stream");
		}

		[HttpPut("user/{id}/{filename}")]
		public async Task<IActionResult> UpdateUserfile(string id, string filename)
		{
			await cloudstorageService.UpdateFileAsync(EpicID.FromString(id), filename, Request.Body);
			return Ok();
		}

		[HttpGet("system")]
		public Task<IActionResult> ListSystemfiles()
		{
			return ListUserfiles(EpicID.Empty.ToString());
		}

		[HttpGet("system/{filename}")]
		public async Task<IActionResult> GetSystemfile(string filename)
		{
			return await GetUserfile(EpicID.Empty.ToString(), filename);
		}
	}
}
