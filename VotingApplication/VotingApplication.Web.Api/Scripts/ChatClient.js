define(['signalRHubs'], function (signalRHubs) {
    function ChatClient() {
        // Proxy to the chatHub signalR Hub
        var chat = signalRHubs.chatHub;
        var self = this;

        // Broadcast message called when a new chat message is sent
        chat.client.broadcastMessage = function (message) {
            if (self.onMessage) self.onMessage(message);
        };
        chat.client.broadcastMessages = function (messages) {
            if (self.onMessages) self.onMessages(messages);
        };
        chat.client.reportError = function (error) {
            console.error(error);
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
        self.sendMessage = function sendMessage(pollId, voterName, message) {
            chat.server
                .sendMessage(pollId, voterName, message)
                .fail(function (x) {
                    console.error(x);
                });
        };

        // Callback functions when receiving messages
        self.onMessage = null;
        self.onMessages = null;
    };
    return new ChatClient();
});