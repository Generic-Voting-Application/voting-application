using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace VotingApplication.Web.Api.Sockets
{
    public class ChatHub : Hub
    {
        public void JoinPoll(Guid pollId)
        {
            Groups.Add(Context.ConnectionId, pollId.ToString());

            // TODO: Send caller the message log so far
            // Clients.Caller.broadcastMessages(chatMessages);
        }

        public void SendMessage(Guid pollId, string name, string message)
        {
            // TODO: Add message to database

            // Broadcast to group
            Clients.Group(pollId.ToString()).broadcastMessage(name, message, DateTime.Now);
        }
    }
}