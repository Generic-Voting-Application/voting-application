define('RankedVote', ['jquery', 'knockout', 'jqueryUI', 'Common', 'PollOptions', 'ResultChart'], function ($, ko, jqueryUI, Common, PollOptions) {

    return function RankedVote(pollId, token) {

        var self = this;
        self.pollOptions = new PollOptions(pollId);
        self.selectedOptions = ko.observableArray();
        self.remainOptions = ko.observableArray();

        self.chartData = ko.observableArray();
        self.winVotesRequired = ko.observable(0);
        self.winners = ko.observableArray();

        self.chartVisible = ko.observable(false);

        var selectPickedOptions = function (votes) {
            self.selectedOptions([]);
            self.remainOptions([]);

            var selected = votes.map(function (vote) {
                return ko.utils.arrayFirst(self.pollOptions.options(), function (item) {
                    return item.Id === vote.OptionId;
                });
            });
            self.selectedOptions(selected);

            var notSelected = function (option) {
                return self.selectedOptions().filter(function (o) { return o.Id === option.Id; }).length === 0;
            };

            self.remainOptions(self.pollOptions.options().filter(notSelected));
        };

        var orderedNames = [];
        var chart;
        var roundIndex = 0;

        var sortByVoteValue = function (a, b) {
            return a.VoteValue - b.VoteValue;
        };

        var sortByBallotCount = function (a, b) {
            return a.ballots.length - b.ballots.length;
        };

        var countVotes = function (votes) {
            var options = [];
            var ballots = [];
            var totalBallots = 0;
            var totalOptions = self.pollOptions.options().length;
            var resultsByRound = [];

            for (var k = 0; k < totalOptions; k++) {
                var optionId = self.pollOptions.options()[k].Id;
                options[k] = { Id: optionId, ballots: [] };
            }

            // Group votes into ballots (per user)
            votes.forEach(function (vote) {
                if (!ballots[vote.UserId]) {
                    ballots[vote.UserId] = [];
                    totalBallots++;
                }
                ballots[vote.UserId].push(vote);
            });

            // Start counting
            while (options.length > 0) {

                // Clear out all ballots from previous round
                options = options.map(function (d) {
                    d.ballots = [];
                    return d;
                });

                var availableOptions = options.map(function (d) { return d.Id; });

                // Sort the votes on the ballots and assign each ballot to first choice
                ballots.forEach(function (ballot) {
                    ballot.sort(sortByVoteValue);
                    var availableChoices = ballot.filter(function (option) { return $.inArray(option.OptionId, availableOptions) !== -1; });
                    if (availableChoices.length > 0) {
                        var firstChoiceId = availableChoices[0].OptionId;
                        var firstChoiceOption = options.filter(function (option) { return option.Id === firstChoiceId; })[0];
                        firstChoiceOption.ballots.push(ballot);
                    }
                });

                options.sort(sortByBallotCount);

                //Convert into a chartable style
                var roundOptions = options.map(function (d) {
                    var matchingOption = $.grep(self.pollOptions.options(), function (opt) { return opt.Id === d.Id; })[0];
                    return {
                        Name: matchingOption.Name,
                        Sum: d.ballots.length,
                        Voters: d.ballots.map(function (x) { return x[0].VoterName + " (#" + (x.map(function (y) { return y.OptionId; }).indexOf(matchingOption.Id) + 1) + ")"; })
                    };
                });

                var roundSeries = {
                    name: 'Round ' + (resultsByRound.length + 1).toString(),
                    data: roundOptions
                };

                // End if we have a majority
                if (options[options.length - 1].ballots.length > totalBallots / 2) {

                    //Mark all other remaining results as having lost
                    for (var i = 0; i < options.length - 1; i++) {
                        options[i].rank = 2;
                    }

                    resultsByRound.push(roundSeries);
                    break;
                }

                if (options[0].ballots.length > 0) {
                    resultsByRound.push(roundSeries);
                }

                // Remove all last place options
                var lastPlaceOption = options[0];
                var lastPlaceBallotCount = lastPlaceOption.ballots.length;

                options = options.filter(function (d) { return d.ballots.length > lastPlaceBallotCount; });
            }

            // Transpose the rounds with options data into options with rounds for the chart
            var resultsByOption = self.pollOptions.options().map(function (option) {
                // Get the per-round results for this option
                var roundData = resultsByRound.map(function (round) {
                    var roundOption = round.data.filter(function (o) { return o.Name === option.Name; });
                    if (roundOption.length > 0) {
                        return {
                            Name: round.name,
                            Sum: roundOption[0].Sum,
                            Voters: roundOption[0].Voters
                        };
                    } else {
                        return {
                            Name: round.name,
                            Sum: 0,
                            Voters: []
                        };
                    }
                });

                return {
                    Name: option.Name,
                    Data: roundData
                };
            });

            return resultsByOption;
        };

        self.onVoted = null;
        self.doVote = function () {
            var userId = Common.currentUserId(pollId);

            if (userId && pollId) {
                var useToken = token || Common.sessionItem("token", pollId);
                // Convert selected options to ranked votes
                var selectedOptionsArray = self.selectedOptions().map(function (option, index) {
                    return {
                        OptionId: option.Id,
                        VoteValue: index + 1,
                        TokenGuid: useToken
                    };
                });

                $.ajax({
                    type: 'PUT',
                    url: '/api/user/' + userId + '/poll/' + pollId + '/vote',
                    contentType: 'application/json',
                    data: JSON.stringify(selectedOptionsArray),

                    success: function () {
                        if (self.onVoted) self.onVoted();
                    },

                    error: Common.handleError
                });
            }
        };

        self.getVotes = function (pollId, userId) {
            $.ajax({
                type: 'GET',
                url: '/api/user/' + userId + '/poll/' + pollId + '/vote',
                contentType: 'application/json',

                success: function (data) {
                    data.sort(sortByVoteValue);
                    selectPickedOptions(data);
                },

                error: Common.handleError
            });
        };

        self.displayResults = function (votes) {
            var groupedVotes = countVotes(votes);

            // Sort based on the relative performance in the last round
            // that either option had votes
            groupedVotes.sort(function (a, b) {
                var index = Math.min(a.Data.length, b.Data.length) - 1;
                while (index > 0 && a.Data[index].Sum === 0 && b.Data[index].Sum === 0) {
                    index--;
                }
                return a.Data[index].Sum - b.Data[index].Sum;
            });

            // Count the voters based on round one
            var voterCount = groupedVotes.reduce(function (sum, group) {
                return sum + group.Data[0].Sum
            }, 0);
            self.winVotesRequired(Math.ceil(voterCount / 2));

            self.chartData(groupedVotes);

            // Store the winners' names (may be a tie)
            self.winners(self.pollOptions.getWinners(groupedVotes, function (group) {
                // Return the option's result in the final round
                return {
                    Name: group.Name,
                    Sum: group.Data[group.Data.length - 1].Sum
                };
            }));
        };

        self.initialise = function (pollData) {

            self.pollOptions.initialise(pollData);

            $(".sortable").sortable({
                items: 'tbody > tr:not(#newOptionRow)',
                connectWith: '.sortable',
                axis: 'y',
                dropOnEmpty: true,
                stop: function () {
                    var votes = [];
                    $('#selectionTable tr.clickable').each(function (i, row) {
                        votes.push({
                            OptionId: parseInt($(row).attr('data-id'))
                        });
                    });

                    // Cancel the sort operation and update the Knockout
                    // arrays, letting Knockout re-arrange the DOM
                    $(".sortable").sortable("cancel");
                    $(".sortable tr.clickable").remove();

                    selectPickedOptions(votes);
                }
            });
        };
        
    };
});