using System;

namespace index.Helpers
{
    public class IndexImportException : Exception
    {
        public IndexImportException(string message) : base(message)
        {
        }
    }
}