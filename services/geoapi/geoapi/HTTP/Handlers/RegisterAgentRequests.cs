using log4net;

namespace geoapi.HTTP.Handlers
{
    public class RegisterAgentRequests
    {
        public string AgentName { get; set; }

        public RegisterAgentRequests(string agentName, ILoggerProvider loggerProvider = null)
        {
            AgentName = agentName;

            var log = loggerProvider?.GetLog() ?? LogManager.GetLogger(GetType());
            log.Info("Recived upload object request");
        }


    }
}