using Fleck;
using System;
using System.Collections.Generic;
using System.Net;

namespace VotingApplication.SocketServer
{
    public class SocketServer
    {
        static void Main(string[] args)
        {
            var connections = new List<IWebSocketConnection>();
            var server = new WebSocketServer("ws://0.0.0.0:5000");
            server.Start(socket =>
            {
                socket.OnOpen = () => connections.Add(socket);
                socket.OnClose = () => connections.Remove(socket);
                socket.OnMessage = message =>
                {
                    foreach (IWebSocketConnection connection in connections)
                        connection.Send(message);
                };
            });
            Console.ReadLine();
        }
    }
}
