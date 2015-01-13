define(['jquery', 'knockout', 'bootstrap', 'insight', 'countdown', 'moment', 'Common', 'ChatClient', 'platform'], function ($, ko, bs, insight, countdown, moment, Common, chatClient) {
    return function VoteViewModel(pollId, token, VotingStrategyViewModel) {
        var self = this;

        var lockCollapse = false;
        var selectedPanel = null;
        var lastResultsRequest = 0;

        token = token || Common.sessionItem("token", pollId);

        self.votingStrategy = null;

        self.pollId = pollId;
        self.pollName = ko.observable("Poll Name");
        self.pollCreator = ko.observable("Poll Creator");
        self.chatMessages = ko.observableArray();
        self.lastMessageId = 0;
        self.userName = ko.observable(Common.currentUserName(pollId));
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
            var username = $("#loginUsername").val();

            $.ajax({
                type: 'PUT',
                url: '/api/user',
                contentType: 'application/json',
                data: JSON.stringify({
                    Name: username,
                    PollId: pollId,
                    Token: {
                        TokenGuid: token
                    }
                }),

                success: function (data) {
                    Common.loginUser(data, username, pollId);
                    self.userName(username);
                    self.userId = Common.currentUserId(pollId);
                    if (!self.pollExpired()) {
                        showSection($('#voteSection'));
                    } else {
                        showSection($('#resultSection'));
                    }
                },

                error: [Common.handleError, function (jqXHR, textStatus, errorThrown) {
                    if (jqXHR.status == 400) {
                        $('#loginSection').addClass("has-error");
                        $('#usernameWarnMessage').show();
                    }
                }]
            });
        };

        self.logout = function () {
            Common.logoutUser(pollId);
            self.userName(undefined);
            self.userId = undefined;
        }

        self.receivedChatMessage = function (name, message, timeStamp) {
            self.chatMessages.push({
                User: { Name: name },
                Message: message,
                Timestamp: new moment(timeStamp).format('H:mm')
            });
            scrollChatWindow();
        };
        self.sendChatMessage = function (data, event) {
            if (self.userId && pollId) {
                var chatMessage = $('#chatTextInput').val();
                $('#chatTextInput').val('');

                chatClient.sendMessage(pollId, self.userName(), chatMessage);
            }
        };
        chatClient.joinPoll(pollId);
        chatClient.onMessage = self.receivedChatMessage;

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
            var userId = Common.currentUserId(pollId);

            var voteData = JSON.stringify([]);

            if (userId && pollId) {
                $.ajax({
                    type: 'PUT',
                    url: '/api/user/' + userId + '/poll/' + pollId + '/vote',
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
                self.votingStrategy.getVotes(pollId, self.userId);
            }
        });

        $('#resultSection .accordion-body').on('show.bs.collapse', function () {
            if (self.votingStrategy) {
                self.getResults(pollId);
            }
        });

        $(document).ready(function () {
            self.userId = Common.currentUserId(pollId);

            getPollDetails(pollId, function () {
                if (self.userId) {
                    $('#loginUsername').val(Common.currentUserName(pollId));
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
            self.votingStrategy = new VotingStrategyViewModel(pollId, token);
        }

        ko.applyBindings(this);
    }
});
