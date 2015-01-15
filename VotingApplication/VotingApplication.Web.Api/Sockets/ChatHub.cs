using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Microsoft.AspNet.SignalR;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Sockets
{
    public class ChatHub : Hub
    {
        private IContextFactory _contextFactory;
        public ChatHub(IContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }
        public virtual string ConnectionId
        {
            get { return Context.ConnectionId; }
        } 

        public void JoinPoll(Guid pollId)
        {
            // Join the group for future messages
            Groups.Add(ConnectionId, pollId.ToString());

            try
            {
                // Send caller the message log so far
                SendChatLog(pollId);
            }
            catch (InvalidOperationException e)
            {
                // Send the error to the caller
                Clients.Caller.reportError(e.Message);
            }
        }

        public void SendMessage(Guid pollId, long userId, string message)
        {
            try
            {
                // Add the new message to the database
                var newMessage = AddNewMessage(pollId, userId, message);

                // Broadcast the new message to group
                Clients.Group(pollId.ToString()).broadcastMessage(newMessage);
            }
            catch (InvalidOperationException e)
            {
                // Send the error to the caller
                Clients.Caller.reportError(e.Message);
            }
        }

        private void SendChatLog(Guid pollId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll matchingPoll = context.Polls.Where(p => p.UUID == pollId).Include(p => p.ChatMessages.Select(m => m.User)).FirstOrDefault();
                if (matchingPoll == null)
                {
                    throw new InvalidOperationException(string.Format("Poll {0} not found", pollId));
                }

                var messages = matchingPoll.ChatMessages ?? new List<ChatMessage>();
                // Broadcast to original caller only
                Clients.Caller.broadcastMessages(messages);
            }
        }

        private ChatMessage AddNewMessage(Guid pollId, long userId, string message)
        {
            var newMessage = new ChatMessage
            {
                User = new User { Id = userId },
                Message = message
            };

            using (var context = _contextFactory.CreateContext())
            {
                if (newMessage == null)
                {
                    throw new InvalidOperationException("Message must not be empty");
                }
                if (newMessage.User == null || newMessage.User.Id == 0)
                {
                    throw new InvalidOperationException("Message requires a user");
                }
                if (string.IsNullOrEmpty(newMessage.Message))
                {
                    throw new InvalidOperationException("Message text required");
                }

                // Lookup the UserId
                User matchingUser = context.Users.Where(u => u.Id == newMessage.User.Id).FirstOrDefault();
                if (matchingUser == null)
                {
                    throw new InvalidOperationException(string.Format("User {0} not found", newMessage.User.Id));
                }

                // Lookup the matching Poll
                Poll matchingPoll = context.Polls.Where(p => p.UUID == pollId).FirstOrDefault();
                if (matchingPoll == null)
                {
                    throw new InvalidOperationException(string.Format("Poll {0} not found", pollId));
                }

                // Get the message ready
                newMessage.User = matchingUser;
                newMessage.Timestamp = DateTime.Now;

                // Add to the list
                if (matchingPoll.ChatMessages == null)
                {
                    matchingPoll.ChatMessages = new List<ChatMessage>();
                }
                matchingPoll.ChatMessages.Add(newMessage);

                context.SaveChanges();
            }

            return newMessage;
        }
    }
}