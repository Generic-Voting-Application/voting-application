define(["mockjax", "Common"], function (mockjax, Common) {
    describe("Poll", function () {

        //#region Setup
        var target;
        var chatWindowMock;
        var socialMock;

        var pastDate, futureDate;

        beforeEach(function (done) {
            // Setup mockjax
            $.mockjaxSettings.responseTime = 1;
            mockjax.clear();

            // Define a mock for ChatClient to be injected with RequireJS
            chatWindowMock = jasmine.createSpy();
            define("ChatWindow", [], function () { return chatWindowMock; });

            // Define a mock for SocialMedia to be injected with RequireJS
            socialMock = jasmine.createSpyObj('SocialMedia', ['googleLogin', 'facebookLogin']);
            define("SocialMedia", [], function () { return socialMock; });
            
            // Poll creates an instance of VotingStrategy that we'll need to spy on
            var VotingStrategyMock = function VotingStrategyMock(pollId) {
                this.pollId = pollId;
                this.initialise = function () { };
                this.displayResults = function () { };
                this.getPreviousVotes = function () { };
            };

            // Create an instance of the test target with the mock dependencies
            require(["Poll"], function (Poll) {
                target = new Poll("test-poll-1", "test-token-453", VotingStrategyMock);

                done();
            });

            // Some test dates to use
            var pastDateObj = new Date();
            pastDateObj.setDate(pastDateObj.getDate() - 1);
            pastDate = pastDateObj.toJSON();

            var futureDateObj = new Date();
            futureDateObj.setDate(futureDateObj.getDate() + 1);
            futureDate = futureDateObj.toJSON();
        });

        afterEach(function () {
            // Remove the mocked objects and target class from RequireJS
            require.undef("SocialMedia");
            require.undef("ChatWindow");
            require.undef("Poll");
        });

        //#endregion

        it("pollExpired without pollExpires expect False", function () {
            // arrange
            target.pollExpires(false);
            target.pollExpiryDate(new Date(pastDate));

            // act/assert
            expect(target.pollExpired()).toBe(false);
        });

        it("pollExpired with pollExpires and Future Date expect False", function () {
            // arrange
            target.pollExpires(true);
            target.pollExpiryDate(new Date(futureDate));

            // act/assert
            expect(target.pollExpired()).toBe(false);
            expect(target.pollExpiryOffset()).toEqual("1 day");
        });

        it("pollExpired with pollExpires and Past Date expect True", function () {
            // arrange
            target.pollExpires(true);
            target.pollExpiryDate(new Date(pastDate));

            // act/assert
            expect(target.pollExpired()).toBe(true);
            expect(target.pollExpiryOffset()).toEqual(null);
        });

        it("setupPollScreen expect Create Voting Strategy", function (done) {
            // arrange
            mockjax({
                type: "GET", url: "/api/poll/test-poll-1",
                responseText: {}
            });
            spyOn(target, "setupPollExpiryTimer");
            spyOn(Common, "getVoterName").and.returnValue(null);

            target.showSection(target.pollSections.vote);

            // act
            target.setupPollScreen();

            // assert
            setTimeout(function () {
                expect(target.votingStrategy.pollId).toEqual("test-poll-1");
                expect(target.showSection()).toEqual(target.pollSections.login);

                done();
            }, 10);
        });

        it("setupPollScreen with Server Error expect Handle Error", function (done) {
            // arrange
            mockjax({
                type: "GET", url: "/api/poll/test-poll-1",
                status: 500, responseText: "Error"
            });
            spyOn(Common, 'handleError');

            // act
            target.setupPollScreen();

            // assert
            setTimeout(function () {
                expect(Common.handleError).toHaveBeenCalled();

                done();
            }, 10);
        });

        it("setupPollScreen expect Get Poll Details", function (done) {
            // arrange
            mockjax({
                type: "GET", url: "/api/poll/test-poll-1",
                responseText: {
                    Name: "test-poll", Creator: "test-creator",
                    RequireAuth: true, Expires: false
                }
            });
            spyOn(target, "setupPollExpiryTimer");
            spyOn(Common, "getVoterName").and.returnValue(null);

            target.showSection(target.pollSections.vote);

            // act
            target.setupPollScreen();

            // assert
            setTimeout(function () {
                expect(target.pollName()).toEqual("test-poll");
                expect(target.pollCreator()).toEqual("test-creator");
                expect(target.requireAuth()).toBe(true);
                expect(target.pollExpires()).toBe(false);

                expect(target.showSection()).toEqual(target.pollSections.login);

                expect(target.setupPollExpiryTimer.calls.count()).toEqual(0);

                done();
            }, 10);
        });

        it("setupPollScreen with Expires expect Setup Expiry Timer", function (done) {
            // arrange
            mockjax({
                type: "GET", url: "/api/poll/test-poll-1",
                responseText: {
                    RequireAuth: false, Expires: true,
                    ExpiryDate: futureDate
                }
            });
            spyOn(target, "setupPollExpiryTimer");
            spyOn(Common, "getVoterName").and.returnValue(null);

            // act
            target.setupPollScreen();

            // assert
            setTimeout(function () {
                expect(target.requireAuth()).toBe(false);
                expect(target.pollExpires()).toBe(true);
                expect(target.userName()).toEqual("");
                expect(target.pollExpiryDate()).toEqual(new Date(futureDate));

                expect(target.showSection()).toEqual(target.pollSections.login);

                expect(target.setupPollExpiryTimer).toHaveBeenCalled();

                done();
            }, 10);
        });

        it("setupPollScreen with Voter expect Login", function (done) {
            // arrange
            mockjax({
                type: "GET", url: "/api/poll/test-poll-1",
                responseText: { Expires: true, ExpiryDate: futureDate }
            });
            spyOn(target, "setupPollExpiryTimer");
            spyOn(Common, "getVoterName").and.returnValue("Bob Tester");
            spyOn(target, "submitLogin");

            // act
            target.setupPollScreen();

            // assert
            setTimeout(function () {
                expect(target.enteredName()).toEqual("Bob Tester");
                expect(target.submitLogin).toHaveBeenCalled();

                done();
            }, 10);
        });

        it("onVoted expect Show Results Section", function (done) {
            // arrange
            mockjax({
                type: "GET", url: "/api/poll/test-poll-1",
                responseText: {}
            });
            spyOn(target, "setupPollExpiryTimer");
            spyOn(Common, "getVoterName").and.returnValue("Bob Tester");

            target.setupPollScreen();

            setTimeout(function () {
                expect(target.showSection()).toEqual(target.pollSections.vote);

                // act
                target.votingStrategy.onVoted();

                // assert
                expect(target.showSection()).toEqual(target.pollSections.results);

                done();
            }, 10);
        });

        it("facebookLogin with Reponse expect Set User and Login", function (done) {
            // arrange
            socialMock.facebookLogin.and.callFake(function (responseFn) {
                // Respond ansynchronously
                setTimeout(function () { responseFn("Bob Facebook");  }, 5);
            });
            spyOn(target, 'submitLogin');

            // act
            target.facebookLogin();

            // assert
            setTimeout(function () {
                expect(target.enteredName()).toEqual("Bob Facebook");
                expect(target.submitLogin).toHaveBeenCalled();

                done();
            }, 10);
        });

        it("googleLogin with Reponse expect Set User and Login", function (done) {
            // arrange
            socialMock.googleLogin.and.callFake(function (responseFn) {
                // Respond ansynchronously
                setTimeout(function () { responseFn("Bob Google"); }, 5);
            });
            spyOn(target, 'submitLogin');

            // act
            target.googleLogin();

            // assert
            setTimeout(function () {
                expect(target.enteredName()).toEqual("Bob Google");
                expect(target.submitLogin).toHaveBeenCalled();

                done();
            }, 10);
        });

        it("submitLogin without Expired expect Set Name and Vote", function (done) {
            // arrange
            spyOn(Common, 'setVoterName');
            spyOn(Common, 'resolveToken').and.callFake(function (poll, uriToken, callback) {
                if (poll === "test-poll-1" && uriToken === "test-token-453") {
                    // Callback asynchronously
                    setTimeout(function () { callback(); }, 5);
                }
            });

            target.enteredName("Bob Tester");
            target.showSection(target.pollSections.login);

            target.pollExpires(false);

            // act
            target.submitLogin();

            // assert
            setTimeout(function () {
                expect(Common.setVoterName).toHaveBeenCalledWith("Bob Tester", "test-poll-1");
                expect(target.showSection()).toEqual(target.pollSections.vote);

                done();
            }, 10);
        });

        it("submitLogin with Expired expect Set Name and Results", function (done) {
            // arrange
            spyOn(Common, 'setVoterName');
            spyOn(Common, 'resolveToken').and.callFake(function (poll, uriToken, callback) {
                if (poll === "test-poll-1" && uriToken === "test-token-453") {
                    // Callback asynchronously
                    setTimeout(function () { callback(); }, 5);
                }
            });

            target.enteredName("Bob Tester");
            target.showSection(target.pollSections.login);

            target.pollExpires(true);
            target.pollExpiryDate(new Date(pastDate));

            // act
            target.submitLogin();

            // assert
            setTimeout(function () {
                expect(Common.setVoterName).toHaveBeenCalledWith("Bob Tester", "test-poll-1");
                expect(target.showSection()).toEqual(target.pollSections.results);

                done();
            }, 10);
        });

        it("logout expect Clear Name and Show Login", function () {
            // arrange
            spyOn(Common, 'clearStorage');

            target.enteredName("Bob Tester");
            target.userName("Bob Tester");
            target.showSection(target.pollSections.vote);

            // act
            target.logout();

            // assert
            expect(Common.clearStorage).toHaveBeenCalledWith("test-poll-1");
            expect(target.enteredName()).toEqual("");
            expect(target.userName()).toEqual("");
            expect(target.showSection()).toEqual(target.pollSections.login);
        });

        it("getResults with Last Update Time expect Display Results", function (done) {
            // arrange
            mockjax({
                type: "GET", url: "/api/poll/test-poll-1/vote?lastPoll=100",
                responseText: { test: 'test-1' }
            });

            // Set a value for the previous update time
            target.lastResultsRequest = 100;
            target.votingStrategy = jasmine.createSpyObj('VotingStrategy', ['displayResults']);
            spyOn(target, 'setupResultsRefresh');

            // act
            target.getResults();

            // assert
            setTimeout(function () {
                expect(Date.now() - target.lastResultsRequest).toBeLessThan(50);
                expect(target.votingStrategy.displayResults).toHaveBeenCalledWith({ test: 'test-1' });
                expect(target.setupResultsRefresh).toHaveBeenCalled();

                done();
            }, 10);
        });

        it("getResults with with Server Error expect Handle Error", function (done) {
            // arrange
            mockjax({
                type: "GET", url: "/api/poll/test-poll-1/vote?lastPoll=0",
                status: 500, responseText: "Error"
            });
            spyOn(Common, 'handleError');

            // act
            target.getResults();

            // assert
            setTimeout(function () {
                expect(Common.handleError).toHaveBeenCalled();

                done();
            }, 10);
        });
        
        it("clearVote with Token expect Clear and Show Results", function (done) {
            // arrange
            spyOn(Common, 'getToken').and.callFake(function (pollId) {
                if (pollId === 'test-poll-1') return 'test-token-782';
            });

            var posted;
            mockjax({
                type: "PUT", url: "/api/token/test-token-782/poll/test-poll-1/vote",
                data: JSON.stringify([]),
                response: function () { posted = true; }
            });

            target.lastResultsRequest = 100;
            target.showSection(target.pollSections.vote);

            // act
            target.clearVote();

            // assert
            setTimeout(function () {
                expect(posted).toBe(true);
                expect(target.lastResultsRequest).toBe(0);
                expect(target.showSection()).toEqual(target.pollSections.results);
                done();
            }, 10);
        });


        it("clearVote with with Server Error expect Handle Error", function (done) {
            // arrange
            spyOn(Common, 'getToken').and.callFake(function (pollId) {
                if (pollId === 'test-poll-1') return 'test-token-782';
            });

            mockjax({
                type: "PUT", url: "/api/token/test-token-782/poll/test-poll-1/vote",
                status: 500, responseText: "Error"
            });
            spyOn(Common, 'handleError');

            // act
            target.clearVote();

            // assert
            setTimeout(function () {
                expect(Common.handleError).toHaveBeenCalled();

                done();
            }, 10);
        });

        it("getPreviousVotes expect Get Previous Votes", function () {
            // arrange
            target.votingStrategy = jasmine.createSpyObj("VotingStrategy", ["getPreviousVotes"]);

            // act
            target.getPreviousVotes();

            // assert
            expect(target.votingStrategy.getPreviousVotes).toHaveBeenCalled();
        });
    });
});
