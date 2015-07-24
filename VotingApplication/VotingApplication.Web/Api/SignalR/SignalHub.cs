using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace VotingApplication.Web.Api.SignalR
{
    [HubName("SignalHub")]
    public class SignalHub : Hub
    {
        public void RegisterObserver(string resourceIdentifier)
        {
            ClientSignaller.RegisterObserver(Context.ConnectionId, resourceIdentifier);
        }
    }
}