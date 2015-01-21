define('BasicVote', ['jquery', 'knockout', 'Common', 'PollOptions', 'ResultChart'], function ($, ko, Common, PollOptions) {
    return function BasicVote(pollId, token) {

        var self = this;
        self.pollOptions = new PollOptions(pollId);

        self.chartData = ko.observableArray();

        var chart;
        var anonymousPoll = true;

        var highlightOption = function (optionId) {
            ko.utils.arrayForEach(self.pollOptions.options(), function (o) {
                o.highlight(o.Id === optionId);
            });
        };

        var countVotes = function (votes) {
            var totalCounts = [];
            votes.forEach(function (vote) {
                var optionName = vote.OptionName;
                var voter = vote.VoterName;

                // Find a vote with the same Option.Name, if it exists.
                var existingOption = totalCounts.filter(function (vote) { return vote.Name === optionName; }).pop();

                if (existingOption) {
                    existingOption.Sum++;
                    existingOption.Voters.push(voter);
                }
                else {
                    totalCounts.push({
                        Name: optionName,
                        Sum: 1,
                        Voters: [voter]
                    });
                }
            });
            return totalCounts;
        };

        self.onVoted = null;
        self.doVote = function (data) {
            var userId = Common.currentUserId(pollId);

            var voteData = JSON.stringify([{
                OptionId: data.Id,
                TokenGuid: token || Common.sessionItem("token", pollId)
            }]);

            if (userId && pollId) {
                $.ajax({
                    type: 'PUT',
                    url: '/api/user/' + userId + '/poll/' + pollId + '/vote',
                    contentType: 'application/json',
                    data: voteData,

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
                    if (data[0]) {
                        highlightOption(data[0].OptionId);
                    }
                    else {
                        highlightOption(-1);
                    }
                },

                error: Common.handleError
            });
        };

        self.displayResults = function (data) {
            var groupedVotes = countVotes(data);
            self.chartData([{ Data: groupedVotes }]);
        };

        self.initialise = function (pollData) {
            self.pollOptions.initialise(pollData);
        };
    };

});