using System;
using indexReact.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace indexReact.Controllers
{
    public class FilesController : ControllerBase
    {
        private readonly IIndexHelper indexHelper;

        public FilesController(IIndexHelper indexHelper)
        {
            this.indexHelper = indexHelper;
        }

        [HttpPost]
        public ActionResult UploadZip()
        {
            if (IsSessionValid())
                return StatusCode(403);

            var files = Request.Form.Files;
            if (files.Count != 1)
                return ThrowError("wrong files count");

            var file = files[0];
            if (!file.FileName.EndsWith(".zip"))
                return ThrowError("wrong file extension");
            if (file.FileName.Contains("/"))
                return ThrowError("no slashes in file name");

            Request.Cookies.TryGetValue(LoginKey, out var login);
            try
            {
                indexHelper.AddToIndex(login, file);
            }
            catch (IndexImportException e)
            {
                return ThrowError(e.Message);
            }
            catch (Exception)
            {
                return ThrowError("ooops");
            }

            return StatusCode(202);
        }

        [HttpGet]
        public ActionResult FindFile(string fileName)
        {
            if (IsSessionValid())
                return StatusCode(403);

            return Json(indexHelper.FindFile(fileName));
        }
    }
}