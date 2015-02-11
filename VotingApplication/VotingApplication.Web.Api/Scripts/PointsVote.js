define('PointsVote', ['jquery', 'knockout', 'Common', 'PollOptions', 'PointsOption', 'ResultChart'], function ($, ko, Common, PollOptions, PointsOption) {

    return function PointsVote(pollId) {

        var self = this;
        self.pollOptions = new PollOptions(pollId);

        self.maxPerVote = ko.observable();
        self.maxPoints = ko.observable();
        self.pointsArray = ko.observableArray();

        self.chartData = ko.observableArray();
        self.winners = ko.observableArray();

        var chart;

        self.pointsForOption = function (index) {
            return self.pointsArray()[index];
        };

        self.pointsRemaining = ko.computed(function () {
            var total = 0;
            ko.utils.arrayForEach(self.pointsArray(), function (item) {
                total += item.value();
            });
            var remaining = self.maxPoints() - total;
            return remaining;
        });

        self.percentRemaining = ko.computed(function () {
            return (self.pointsRemaining() / self.maxPoints()) * 100;
        });

        var resetVote = function () {
            //Populate with an array of options.length number of 0-values
            self.pointsArray(self.pollOptions.options().map(function (o) {
                return new PointsOption(self.maxPerVote, self.pointsRemaining);
            }));
        };

        var countVotes = function (votes) {
            var totalCounts = [];

            votes.sort(function (a, b) {
                return b.VoteValue - a.VoteValue;
            }).forEach(function (vote) {
                var optionName = vote.OptionName;
                var voter = vote.VoterName;
                var voteValue = vote.VoteValue;

                var voteString = voter + " (" + voteValue + ")";

                // Find a vote with the same Option.Name, if it exists.
                var existingOption = totalCounts.filter(function (vote) { return vote.Name == optionName; }).pop();

                if (existingOption) {
                    existingOption.Sum += voteValue;
                    existingOption.Voters.push(voteString);
                }
                else {
                    totalCounts.push({
                        Name: optionName,
                        Sum: voteValue,
                        Voters: [voteString]
                    });
                }
            });

            return totalCounts;
        };

        self.pollOptions.options.subscribe(function () {
            var newOptionCount = self.pollOptions.options().length - self.pointsArray().length;
            for (var i = 0; i < newOptionCount; i++) {
                self.pointsArray.push(new PointsOption(self.maxPerVote, self.pointsRemaining));
            }
        });

        self.onVoted = null;
        self.doVote = function (data, event) {

            var votesData = self.pollOptions.options()
                .map(function (option, i) {
                    return {
                        OptionId: option.Id,
                        VoteValue: self.pointsForOption(i).value(),
                        VoterName: Common.getVoterName()
                    };
                })
                .filter(function (vote) { return vote.VoteValue > 0; });

            var tokenGuid = Common.getToken(pollId);

            if (tokenGuid && pollId) {
                $.ajax({
                    type: 'PUT',
                    url: '/api/token/' + tokenGuid + '/poll/' + pollId + '/vote',
                    contentType: 'application/json',
                    data: JSON.stringify(votesData),

                    success: function (returnData) {
                        if (self.onVoted) self.onVoted();
                    },

                    error: Common.handleError
                });
            }
        };

        self.getPreviousVotes = function (pollId) {
            var tokenGuid = Common.getToken(pollId);

            if (tokenGuid && pollId) {

                $.ajax({
                    type: 'GET',
                    url: '/api/token/' + tokenGuid + '/poll/' + pollId + '/vote',
                    contentType: 'application/json',

                    success: function (data) {
                        resetVote();
                        var allOptions = self.pollOptions.options();
                        for (var i = 0; i < data.length; i++) {
                            //Find index of previously voted option
                            var vote = allOptions.filter(function (d) {
                                return d.Id == data[i].OptionId;
                            })[0];
                            var optionIndex = self.pollOptions.options().indexOf(vote);

                            if (optionIndex == -1)
                                continue;

                            self.pointsArray()[optionIndex].value(data[i].VoteValue);
                        }
                    }
                });
            }
        };

        self.displayResults = function (data) {
            var groupedVotes = countVotes(data);
            self.chartData([{ Data: groupedVotes }]);

            // Store the winners' names (may be a tie)
            self.winners(self.pollOptions.getWinners(groupedVotes));
        }

        self.initialise = function (pollData) {
            self.pollOptions.initialise(pollData);

            self.maxPerVote(pollData.MaxPerVote);
            self.maxPoints(pollData.MaxPoints);

            resetVote();
        }

    }

});
