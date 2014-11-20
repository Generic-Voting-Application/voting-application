require(['jquery', 'knockout', 'bootstrap', 'insight', 'Common'], function ($, ko, bs, insight, Common) {
    function VoteViewModel() {
        var self = this;

        var votingStrategy;

        self.pollName = ko.observable("Poll Name");
        self.pollCreator = ko.observable("Poll Creator");
        self.options = ko.observableArray();

        var getPollDetails = function (pollId) {
            $.ajax({
                type: 'GET',
                url: "/api/session/" + pollId,

                success: function (data) {
                    self.pollName(data.Name);
                    self.pollCreator(data.Creator);

                    //Hardcode
                    data.VotingStrategy = 'Basic';

                    var options = data.Options;

                    switch (data.VotingStrategy) {
                        case 'Basic':
                            pickStrategy('/Partials/VotingStrategies/BasicVote.html', 'BasicVote', options);
                    }
                }
            });
        };

        var pickStrategy = function (htmlFile, votingStrategy, options) {

            // Load partial HTML
            $.ajax({
                type: 'GET',
                url: htmlFile,
                dataType: 'html',

                success: function (data) {
                    $("#optionTable").append(data);
                    require([votingStrategy], function (strategy) {
                        votingStrategyFunc(strategy, options);
                    });
                }
            });
        };

        var votingStrategyFunc = function (strategy, options) {
            function StrategyViewModel() {
                votingStrategy = new strategy();
                votingStrategy.options(options);
                return votingStrategy;
            }

            ko.applyBindings(new StrategyViewModel(), $('#optionTable')[0]);
        };

        var showSection = function (element) {
            var siblings = element.siblings();
            for (var i = 0; i < siblings.length; i++) {
                $(siblings[i]).collapseSection('hide');
                $(siblings[i]).removeClass('panel-primary');

            }
            element.collapseSection('show');
            element.addClass('panel-primary');
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
                    Name: username
                }),

                success: function (data) {
                    Common.loginUser(data, username);
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

            getPollDetails(self.pollId);

            if (self.userId) {
                $('#loginUsername').val(Common.currentUserName());
                showSection($('#voteSection'));
            } else {
                showSection($('#loginSection'));
            }
        });
    }

    ko.applyBindings(new VoteViewModel());
});
