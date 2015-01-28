//scrolling window
//Showing sections
//Handling accordian clicks

define('Poll', ['jquery', 'knockout', 'countdown', 'Common', 'SocialMedia', 'ChatWindow', 'KnockoutExtensions'], function ($, ko, countdown, Common, social, ChatWindow) {

    return function VoteViewModel(pollId, uriTokenGuid, VotingStrategyViewModel) {
        var self = this;

        self.lastResultsRequest = 0;

        self.pollSections = {
            vote: 0,
            results: 1,
            login: 2
        };
        self.showSection = ko.observable(self.pollSections.login);

        self.votingStrategy = null;
        self.chatWindow = new ChatWindow(pollId);

        self.pollId = pollId;
        self.pollName = ko.observable("Poll Name");
        self.pollCreator = ko.observable("Poll Creator");
        self.lastMessageId = 0;
        self.userName = ko.observable("");
        self.requireAuth = ko.observable();

        self.pollExpires = ko.observable(false);
        self.pollExpiryDate = ko.observable();
        self.pollExpired = ko.computed(function () {
            return self.pollExpires() && self.pollExpiryDate() <= new Date();
        });
        self.pollExpiryOffset = ko.computed(function () {
            if (!self.pollExpired()) {
                return countdown(self.pollExpiryDate()).toString();
            } else {
                return null;
            }
        });

        self.setupPollExpiryTimer = function () {
            setInterval(function () {
                self.pollExpiryDate.notifySubscribers();
                if (self.pollExpired() && self.userName()) {
                    self.showSection(self.pollSections.results);
                }
            }, 1000);
        };

        var getPollDetails = function () {
            $.ajax({
                type: 'GET',
                url: "/api/poll/" + pollId,

                success: function (data) {
                    self.votingStrategy.initialise(data);

                    self.pollName(data.Name);
                    self.pollCreator(data.Creator);
                    self.requireAuth(data.RequireAuth);
                    self.pollExpires(data.Expires);
                    self.pollExpiryDate(new Date(data.ExpiryDate));

                    if (data.Expires) {
                        self.setupPollExpiryTimer();
                    }

                    var voterName = Common.getVoterName(pollId);
                    if (voterName) {
                        self.userName(voterName);
                        if (!self.pollExpired()) {
                            self.showSection(self.pollSections.vote);
                        } else {
                            self.showSection(self.pollSections.results);
                        }

                    } else {
                        self.showSection(self.pollSections.login);
                    }
                },

                error: Common.handleError
            });
        };

        var socialLoginResponse = function (username) {
            self.userName(username);
            self.submitLogin();
        };

        self.facebookLogin = function () {
            social.facebookLogin(socialLoginResponse);
        };

        self.googleLogin = function () {
            social.googleLogin(socialLoginResponse);
        };

        self.submitLogin = function () {
            Common.setVoterName(self.userName(), pollId);

            if (!self.pollExpired()) {
                self.showSection(self.pollSections.vote);
            } else {
                self.showSection(self.pollSections.results);
            }
        };

        self.logout = function () {
            Common.clearStorage(pollId);
            self.userName("");
            self.showSection(self.pollSections.login);
        };

        self.getResults = function () {
            $.ajax({
                type: 'GET',
                url: '/api/poll/' + pollId + '/vote?lastPoll=' + self.lastResultsRequest,

                statusCode: {
                    200: function (data) {
                        if (self.votingStrategy) {

                            self.lastResultsRequest = Date.now();
                            self.votingStrategy.displayResults(data);
                        }
                    }
                },

                error: Common.handleError
            });
            
            // Setup refresh timer
            self.setupResultsRefresh();
        };

        var refreshInterval;
        self.setupResultsRefresh = function () {
            if (!refreshInterval) {
                refreshInterval = setInterval(self.getResults, 10000);
            }
        };

        self.clearVote = function () {
            var voteData = JSON.stringify([]);

            var tokenGuid = Common.getToken(pollId);

            if (tokenGuid && pollId) {
                $.ajax({
                    type: 'PUT',
                    url: '/api/token/' + Common.getToken(pollId) + '/poll/' + pollId + '/vote',
                    contentType: 'application/json',
                    data: voteData,

                    success: function () {
                        self.lastResultsRequest = 0;

                        self.showSection(self.pollSections.results);
                    },

                    error: Common.handleError
                });
            }
        };

        self.getPreviousVotes = function () {
            self.votingStrategy.getPreviousVotes(pollId);
        };

        self.setupPollScreen = function () {
            if (VotingStrategyViewModel) {
                self.votingStrategy = new VotingStrategyViewModel(pollId);

                self.votingStrategy.onVoted = function () {
                    // Switch to the results panel
                    self.showSection(self.pollSections.results);
                };
            }

            getPollDetails();
        };

        self.initialise = function () {
            Common.resolveToken(pollId, uriTokenGuid);
            self.setupPollScreen();

            ko.applyBindings(this);
        };
    };

});
