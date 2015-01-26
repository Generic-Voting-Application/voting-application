define('ChatWindow', ['knockout', 'moment', 'ChatClient', 'Common'], function (ko, moment, chatClient, Common) {
    return function ChatWindow(pollId) {
        var self = this;
        self.chatMessages = ko.observableArray();
        self.chatMessage = ko.observable("");

        var formatMessage = function (message) {

            // Careful here, startOf modifies the object it is called on.
            messageTimestamp = new moment(message.Timestamp);
            messageDayStart = new moment(message.Timestamp).startOf('day');

            message.Timestamp = messageDayStart.isSame(new moment().startOf('day')) ? messageTimestamp.format('HH:mm') : messageTimestamp.format('DD/MM');
            return message;
        };
        chatClient.onMessage = function (message) {
            self.chatMessages.push(formatMessage(message));
        };
        chatClient.onMessages = function (messages) {
            self.chatMessages(messages.map(formatMessage))
        };

        chatClient.joinPoll(pollId);

        self.sendChatMessage = function (data, event) {
            if (pollId) {
                chatClient.sendMessage(pollId, Common.getVoterName(), self.chatMessage());
                self.chatMessage("");
            }
        };

    };
});
