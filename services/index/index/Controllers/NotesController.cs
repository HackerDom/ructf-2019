using index.db.Models;
using index.db.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace index.Controllers
{
    public class NotesController : ControllerBase
    {
        private readonly IServiceBase<Note> db;
        private readonly IHostingEnvironment hostingEnvironment;

        public NotesController(IServiceBase<Note> db, IHostingEnvironment hostingEnvironment)
        {
            this.db = db;
            this.hostingEnvironment = hostingEnvironment;
        }

        [HttpPost]
        public ActionResult Add([FromBody] NoteModel note)
        {
            if (IsSessionNotValid())
                return StatusCode(403);

            if (note.Note.Length > 100)
                return ThrowError("Too long");

            db.Create(
                new Note
                {
                    OwnerName = GetLogin(),
                    Text = note.Note,
                    IsPublic = note.IsPublic,
                });
            return StatusCode(201);
        }

        [HttpGet]
        public ActionResult Get()
        {
            if (!IsAdminSession() || IsSessionNotValid())
                return StatusCode(403);

            return StatusCode(202);
        }

        private bool IsAdminSession() =>
            !hostingEnvironment.IsProduction() && Request.Cookies.TryGetValue("admin", out _);
    }

    public class NoteModel
    {
        public string Note { get; set; }
        public bool IsPublic { get; set; }
    }
}