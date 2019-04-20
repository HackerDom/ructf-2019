using indexReact.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace indexReact.Controllers
{
    public class ZipController : ControllerBase
    {
        private readonly IIndexHelper indexHelper;

        public ZipController(IIndexHelper indexHelper)
        {
            this.indexHelper = indexHelper;
        }

        [HttpPost]
        public ActionResult UploadZip()
        {
            if (IsSessionValid())
                return StatusCode(403);

            Response.StatusCode = 400;
            var files = Request.Form.Files;
            if (files.Count != 1)
                return Json(new { error = "wrong files count" });

            var file = files[0];
            if (!file.FileName.EndsWith(".zip"))
                return Json(new { error = "wrong file extension" });
            if (file.FileName.Contains("/"))
                return Json(new { error = "no slashes in file name" });

            Request.Cookies.TryGetValue(LoginKey, out var login);
            indexHelper.AddToIndex(login, file);

            return StatusCode(202);
        }
    }
}