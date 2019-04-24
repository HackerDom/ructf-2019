using index.db.Models;
using Microsoft.Extensions.Configuration;

namespace index.db.Services
{
    public class NotesService : ServiceBase<Note>
    {
        public NotesService(IConfiguration config) : base(config, "notes")
        {
        }
    }
}