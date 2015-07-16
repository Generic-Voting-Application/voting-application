using Microsoft.AspNet.SignalR;

namespace VotingApplication.Web.Api.SignalR
{
    public static class ClientSignaller
    {
        private static IHubContext getHubContext()
        {
            return GlobalHost.ConnectionManager.GetHubContext<SignalHub>();
        }

        public static void RegisterObserver(string watcher, string identifier)
        {
            getHubContext().Groups.Add(watcher, identifier.ToLower());
        }

        public static void UnregisterObserver(string watcher, string identifier)
        {
            getHubContext().Groups.Remove(watcher, identifier.ToLower());
        }

        public static void SignalUpdate(string identifier)
        {
            getHubContext().Clients.Group(identifier.ToLower()).update();
        }

    }
}