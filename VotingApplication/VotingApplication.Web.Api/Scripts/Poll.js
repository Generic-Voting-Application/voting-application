//scrolling window
//Showing sections
//Handling accordian clicks

define('Poll', ['jquery', 'knockout', 'countdown', 'Common', 'SocialMedia', 'ChatWindow', 'KnockoutExtensions'], function ($, ko, countdown, Common, social, ChatWindow) {
    return function VoteViewModel(pollId, uriTokenGuid, VotingStrategyViewModel) {
        var self = this;

        var lockCollapse = false;
        var selectedPanel = null;
        var lastResultsRequest = 0;

        self.votingStrategy = null;
        self.chatWindow = new ChatWindow(pollId);

        self.pollId = pollId;
        self.pollName = ko.observable("Poll Name");
        self.pollCreator = ko.observable("Poll Creator");
        self.lastMessageId = 0;
        self.userName = ko.observable(Common.getVoterName(pollId));
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

        var updatePollExpiryTime = function () {
            self.pollExpiryDate.notifySubscribers();
            if (self.pollExpired() && self.userName()) {
                showSection($('#resultSection'));
            }
        }

        var getPollDetails = function (pollId, callback) {
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
                        setInterval(updatePollExpiryTime, 1000);
                    }

                    callback();
                },

                error: Common.handleError
            });
        };

        var showSection = function (element) {
            if (selectedPanel == null || (selectedPanel[0] != element[0])) {
                selectedPanel = element;
                if (!lockCollapse) {
                    lockCollapse = true;
                    var siblings = element.siblings();
                    for (var i = 0; i < siblings.length; i++) {
                        $(siblings[i]).collapseSection('hide');
                        $(siblings[i]).removeClass('panel-primary');
                    }
                    element.collapseSection('show');
                    $(element).on('shown.bs.collapse', function (e) {
                        lockCollapse = false;
                    });
                    element.addClass('panel-primary');
                }
            }
        };

        var socialLoginResponse = function (username) {
            self.userName(username);
            self.submitLogin();
        };
        
        self.facebookLogin = function () {
            social.facebookLogin(socialLoginResponse);
        }

        self.googleLogin = function () {
            social.googleLogin(socialLoginResponse);
        };

        self.showSection = function (data, event) {
            showSection($(data).parent());
        };

        self.submitLogin = function (data, event) {
            Common.setVoterName(self.userName(), pollId);

            if (!self.pollExpired()) {
                showSection($('#voteSection'));
            } else {
                showSection($('#resultSection'));
            }
        };

        self.logout = function () {
            Common.clearStorage(pollId);
            self.userName("");
        }

        self.getResults = function (pollId) {
            $.ajax({
                type: 'GET',
                url: '/api/poll/' + pollId + '/vote?lastPoll=' + lastResultsRequest,

                statusCode: {
                    200: function (data) {
                        if (self.votingStrategy) {

                            lastResultsRequest = Date.now();
                            self.votingStrategy.displayResults(data);
                        }
                    }
                },

                error: Common.handleError
            });
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

                    success: function (returnData) {
                        lastResultsRequest = 0;

                        $('#resultSection > div')[0].click();
                    },

                    error: Common.handleError
                });
            }
        }

        $('#voteSection .accordion-body').on('show.bs.collapse', function () {
            if (self.votingStrategy) {
                self.votingStrategy.getPreviousVotes(pollId);
            }
        });

        $('#resultSection .accordion-body').on('show.bs.collapse', function () {
            if (self.votingStrategy) {
                self.getResults(pollId);
            }
        });

        $(document).ready(function () {
            Common.resolveToken(pollId, uriTokenGuid);

            getPollDetails(pollId, function () {

                var voterName = Common.getVoterName(pollId);
                if (voterName) {
                    self.userName(voterName);
                    if (!self.pollExpired()) {
                        showSection($('#voteSection'));
                    } else {
                        showSection($('#resultSection'));
                    }

                } else {
                    showSection($('#loginSection'));
                }
            });

            setInterval(function () {
                self.getResults(pollId);
            }, 10000);
        });

        if (VotingStrategyViewModel) {
            self.votingStrategy = new VotingStrategyViewModel(pollId);

            self.votingStrategy.onVoted = function () {
                // Switch to the vote panel
                $('#resultSection > div')[0].click();
            };
        }

        ko.applyBindings(this);
    }
});
