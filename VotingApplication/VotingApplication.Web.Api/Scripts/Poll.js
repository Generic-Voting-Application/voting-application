﻿require(['jquery', 'knockout', 'bootstrap', 'insight', 'Common', 'platform'], function ($, ko, bs, insight, Common) {
    function VoteViewModel() {
        var self = this;

        var votingStrategy;
        var lockCollapse = false;

        self.pollName = ko.observable("Poll Name");
        self.pollCreator = ko.observable("Poll Creator");
        self.options = ko.observableArray();
        self.chatMessages = ko.observableArray();
        self.lastMessageId = 0;
        self.userName = ko.observable(Common.currentUserName());
        self.requireAuth = ko.observable();

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

        var getPollDetails = function (pollId, callback) {
            $.ajax({
                type: 'GET',
                url: "/api/poll/" + pollId,

                success: function (data) {
                    self.pollName(data.Name);
                    self.pollCreator(data.Creator);
                    self.requireAuth(data.RequireAuth);

                    switch (data.VotingStrategy) {
                        case 'Basic':
                            loadStrategy('/Partials/VotingStrategies/BasicVote.html', '/Scripts/VotingStrategies/BasicVote.js', data, callback);
                            break;
                        case 'Points':
                            loadStrategy('/Partials/VotingStrategies/PointsVote.html', '/Scripts/VotingStrategies/PointsVote.js', data, callback);
                            break;
                        case 'Ranked':
                            loadStrategy('/Partials/VotingStrategies/RankedVote.html', 'VotingStrategies/RankedVote', data, callback);
                            break;
                    }
                }
            });
        };

        var loadStrategy = function (htmlFile, votingStrategy, pollData, callback) {
            // Load partial HTML
            $.ajax({
                type: 'GET',
                url: htmlFile,
                dataType: 'html',

                success: function (data) {
                    $("#votingArea").append(data);
                    require([votingStrategy], function (strategy) {
                        votingStrategyFunc(strategy, pollData);
                        callback();
                    });
                }
            });
        };

        var votingStrategyFunc = function (strategy, pollData) {
            function StrategyViewModel() {
                votingStrategy = new strategy(pollData.Options, pollData);
                return votingStrategy;
            }

            ko.applyBindings(new StrategyViewModel(), $('#votingArea')[0]);
        };

        var showSection = function (element) {
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
        };

        var scrollChatWindow = function () {
            $("#chat-messages").animate({
                scrollTop: $("#chat-messages")[0].scrollHeight
            });
        };

        var getChatMessages = function () {
            if (self.pollId) {
                var timestamp = Math.floor((new Date).getTime() / 1000);
                var url = 'api/poll/' + self.pollId + '/chat?$orderby=Id&$filter=Id gt ' + self.lastMessageId;

                $.ajax({
                    type: 'GET',
                    'url': url,
                    contentType: 'application/json',
                    success: function (data) {

                        if (data.length > 0) {
                            var messageId = data[data.length - 1].Id
                            if (!self.lastMessageId || messageId > self.lastMessageId) {
                                self.lastMessageId = messageId;
                            }

                            data.forEach(function (message) {
                                self.chatMessages.push(message);
                            });
                            scrollChatWindow();
                        }
                    }
                });


            }
        }

        self.showSection = function (data, event) {
            showSection($(data).parent());
        };

        self.googleLogin = function (authResult) {
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

        self.facebookLogin = function (response) {
            if (response.status != 'connected') {
                return;
            }

            FB.api('/me', function (content) {
                var username = content.first_name + " " + content.last_name;

                //Hijack the regular login
                $("#loginUsername").val(username);
                self.submitLogin();
            })
        }

        self.submitLogin = function (data, event) {
            var username = $("#loginUsername").val();
            $.ajax({
                type: 'PUT',
                url: '/api/user',
                contentType: 'application/json',
                data: JSON.stringify({
                    Name: username
                }),

                success: function (data) {
                    Common.loginUser(data, username);
                    self.userName(username);
                    self.userId = data;
                    showSection($('#voteSection'));
                },

                error: function (jqXHR, textStatus, errorThrown) {
                    if (jqXHR.status == 400) {
                        $('#loginSection').addClass("has-error");
                        $('#usernameWarnMessage').show();

                    }
                }
            });
        };

        self.sendChatMessage = function (data, event) {
            if (self.userId && self.pollId) {
                var url = 'api/poll/' + self.pollId + '/chat';
                var chatMessage = $('#chatTextInput').val();
                $('#chatTextInput').val('');

                $.ajax({
                    type: 'POST',
                    'url': url,
                    contentType: 'application/json',
                    data: JSON.stringify({
                        User: { Id: self.userId },
                        Message: chatMessage
                    })
                });
            }

        }

        $('#voteSection .accordion-body').on('show.bs.collapse', function () {
            if (votingStrategy) {
                votingStrategy.getVotes(self.pollId, self.userId);
            }
        });

        $('#resultSection .accordion-body').on('show.bs.collapse', function () {
            if (votingStrategy) {
                votingStrategy.getResults(self.pollId);
            }
        });

        $(document).ready(function () {
            self.pollId = Common.getPollId();
            self.userId = Common.currentUserId();

            getPollDetails(self.pollId, function () {
                if (self.userId) {
                    $('#loginUsername').val(Common.currentUserName());
                    showSection($('#voteSection'));
                } else {
                    showSection($('#loginSection'));
                }
            });

            getChatMessages();
            setInterval(getChatMessages, 3000);

            // Additional params including the callback, the rest of the params will
            // come from the page-level configuration.
            var additionalParams = {
                'callback': self.googleLogin
            };

            // Attach a click listener to a button to trigger the flow.
            $('#signinGoogleButton')[0].addEventListener('click', function () {
                gapi.auth.signIn(additionalParams); // Will use page level configuration
            });

            $('#signinFacebookButton')[0].addEventListener('click', function () {
                FB.login(function (response) {
                    self.facebookLogin(response);
                });
            })
        });
    }

    ko.applyBindings(new VoteViewModel());
});