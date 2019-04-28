using geoapi2.Models;
using log4net;

namespace geoapi2.HTTP
{
    public class UploadObjectRequest
    {
        public string AgentId { get; set; }
        public CellTypes[,] Cells { get; set; }
        public string Info { get; set; }

        public UploadObjectRequest(string agentId, CellTypes[,] cells, string info, ILoggerProvider loggerProvider = null)
        {
            AgentId = agentId;
            Cells = cells;
            Info = info;

            var log = loggerProvider?.GetLog() ?? LogManager.GetLogger(GetType());
            log.Info("Recived upload object request");

        }
    }
}