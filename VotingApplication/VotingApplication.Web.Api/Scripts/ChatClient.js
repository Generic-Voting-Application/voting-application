define(['signalRHubs'], function (signalRHubs) {
    function ChatClient() {
        // Proxy to the chatHub signalR Hub
        var chat = signalRHubs.chatHub;
        var self = this;

        // Broadcast message called when a new chat message is sent
        chat.client.broadcastMessage = function (name, message, timeStamp) {
            if (self.onMessage) self.onMessage(name, message, timeStamp);
        };

        // Start the connection
        var started = false;

        // Join a particular poll's chat
        self.joinPoll = function joinPoll(pollId) {
            signalRHubs.hub.start().done(function () {
                started = true;
                chat.server.joinPoll(pollId);
            });
        };

        // Send a message to the server
        self.sendMessage = function sendMessage(pollId, name, message) {
            chat.server.sendMessage(pollId, name, message);
        };

        // Callback function when receiving messages
        self.onMessage = null;
    };
    return new ChatClient();
});