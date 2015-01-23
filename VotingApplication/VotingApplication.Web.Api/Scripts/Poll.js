define('Poll', ['jquery', 'knockout', 'bootstrap', 'countdown', 'moment', 'Common', 'ChatClient', 'platform'], function ($, ko, bs, countdown, moment, Common, chatClient) {
    return function VoteViewModel(pollId, uriTokenGuid, VotingStrategyViewModel) {
        var self = this;

        var lockCollapse = false;
        var selectedPanel = null;
        var lastResultsRequest = 0;

        self.votingStrategy = null;

        self.pollId = pollId;
        self.pollName = ko.observable("Poll Name");
        self.pollCreator = ko.observable("Poll Creator");
        self.chatMessages = ko.observableArray();
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

        // Begin Facebook boilerplate

        window.fbAsyncInit = function () {
            FB.init({
                appId: '333351380206896',
                xfbml: true,
                version: 'v2.2'
            });
        };

        (function (d, s, id) {
            var js, fjs = d.getElementsByTagName(s)[0];
            if (d.getElementById(id)) { return; }
            js = d.createElement(s); js.id = id;
            js.src = "//connect.facebook.net/en_US/sdk.js";
            fjs.parentNode.insertBefore(js, fjs);
        }(document, 'script', 'facebook-jssdk'));

        // End Facebook boilerplate

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

        var scrollChatWindow = function () {
            $("#chat-messages").animate({
                scrollTop: $("#chat-messages")[0].scrollHeight
            });
        };

        var googleLogin = function (authResult) {
            //Login failed
            if (!authResult['status']['signed_in']) {
                return;
            }

            //Load the username and login to GVA
            gapi.client.load('plus', 'v1', function () {
                var request = gapi.client.plus.people.get({
                    'userId': 'me'
                });
                request.execute(function (resp) {
                    //Hijack the regular login
                    $("#loginUsername").val(resp.displayName);
                    self.submitLogin();
                });
            });
        }

        self.facebookLogin = function (data, event) {
            FB.login(function (response) {
                if (response.status != 'connected') {
                    return;
                }

                FB.api('/me', function (content) {
                    var username = content.first_name + " " + content.last_name;

                    //Hijack the regular login
                    $("#loginUsername").val(username);
                    self.submitLogin();
                })
            });
        }

        self.googleLogin = function (data, event) {
            gapi.auth.signIn({
                'callback': googleLogin
            });
        };

        self.showSection = function (data, event) {
            showSection($(data).parent());
        };

        self.submitLogin = function (data, event) {
            var userName = $("#loginUsername").val();
            self.userName(userName);
            Common.setVoterName(userName, pollId);

            if (!self.pollExpired()) {
                showSection($('#voteSection'));
            } else {
                showSection($('#resultSection'));
            }
        };

        self.logout = function () {
            Common.clearStorage(pollId);
            self.userName(undefined);
        }

        self.chatMessage = ko.observable("");

        var receivedMessage = function (message) {

            // Careful here, startOf modifies the object it is called on.
            messageTimestamp = new moment(message.Timestamp);
            messageDayStart = new moment(message.Timestamp).startOf('day');

            message.Timestamp = messageDayStart.isSame(new moment().startOf('day')) ? messageTimestamp.format('HH:mm') : messageTimestamp.format('DD/MM');

            self.chatMessages.push(message);
        };
        chatClient.onMessage = function (message) {
            receivedMessage(message);
            scrollChatWindow();
        };
        chatClient.onMessages = function (messages) {
            ko.utils.arrayForEach(messages, receivedMessage);
            scrollChatWindow();
        };

        chatClient.joinPoll(pollId);

        self.sendChatMessage = function (data, event) {
            if (pollId) {
                chatClient.sendMessage(pollId, Common.getVoterName(), self.chatMessage());
                self.chatMessage("");
            }
        };

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
