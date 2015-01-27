define(["Common"], function (Common) {
    describe("ChatWindow", function () {

        //#region Setup
        var target;
        var chatClientMock;

        beforeEach(function (done) {
            // Define a mock for ChatClient to be injected with RequireJS
            chatClientMock = jasmine.createSpyObj('ChatClient', ['joinPoll', 'sendMessage'])
            define("ChatClient", [], function () { return chatClientMock; });

            // Create an instance of the test target with the mock dependencies
            require(["ChatWindow"], function (ChatWindow) {
                target = new ChatWindow("test-poll-1");

                done();
            });
        });

        afterEach(function () {
            // Remove the mocked object and target class from RequireJS
            require.undef("ChatClient");
            require.undef("ChatWindow");
        });

        //#endregion

        it("New with PollId expect Join Poll", function () {
            // assert
            expect(chatClientMock.joinPoll).toHaveBeenCalledWith("test-poll-1");

            expect(target.chatMessages()).toEqual([]);
            expect(target.chatMessage()).toEqual("");
        });

        it("OnMessage with Message Before Today expect Message with Formatted Date", function () {
            // arrange
            var message = { Tag: "test-1", Timestamp: new Date(2014, 11, 25, 14, 45) };

            // act
            chatClientMock.onMessage(message);

            // assert
            expect(target.chatMessages()).toEqual([{ Tag: "test-1", Timestamp: "25/12" }]);
        });

        it("OnMessage with Message Today expect Message with Formatted Time", function () {
            // arrange
            var messageTime = new Date();
            messageTime.setHours(14, 45, 0, 0);
            var message = { Tag: "test-1", Timestamp: messageTime };

            // act
            chatClientMock.onMessage(message);

            // assert
            expect(target.chatMessages()).toEqual([{ Tag: "test-1", Timestamp: "14:45" }]);
        });

        it("OnMessages with Message Today and Before Today expect Messages with Formatted Date and Time", function () {
            // arrange
            var messageTime = new Date();
            messageTime.setHours(14, 45, 0, 0);

            var messages = [
                { Tag: "test-1", Timestamp: new Date(2014, 11, 25, 14, 45) },
                { Tag: "test-2", Timestamp: messageTime }
            ];

            // act
            chatClientMock.onMessages(messages);

            // assert
            expect(target.chatMessages()).toEqual([
                { Tag: "test-1", Timestamp: "25/12" },
                { Tag: "test-2", Timestamp: "14:45" }
            ]);
        });

        it("sendChatMessage expect Send with Voter Name", function () {
            // arrange
            target.chatMessage("A test message");

            spyOn(Common, 'getVoterName').and.returnValue("A Voter");

            // act
            target.sendChatMessage();

            // assert
            expect(chatClientMock.sendMessage).toHaveBeenCalledWith("test-poll-1", "A Voter", "A test message");
        });

    });
});
