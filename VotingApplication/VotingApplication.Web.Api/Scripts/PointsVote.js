define('PointsVote', ['jquery', 'knockout', 'Common', 'PollOptions', 'PointsOption', 'insight'], function ($, ko, Common, PollOptions, PointsOption, insight) {

    return function PointsVote(pollId, token) {

        var self = this;
        self.pollOptions = new PollOptions(pollId);

        self.maxPerVote = ko.observable();
        self.maxPoints = ko.observable();
        self.pointsArray = ko.observableArray();

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
            votes.forEach(function (vote) {
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
            var userId = Common.currentUserId(pollId);

            var useToken = token || Common.sessionItem("token", pollId);
            var votesData = self.pollOptions.options()
                .map(function (option, i) {
                    return {
                        OptionId: option.Id,
                        VoteValue: self.pointsForOption(i).value(),
                        TokenGuid: useToken
                    };
                })
                .filter(function (vote) { return vote.PollValue > 0; });

            if (userId && pollId) {
                $.ajax({
                    type: 'PUT',
                    url: '/api/user/' + userId + '/poll/' + pollId + '/vote',
                    contentType: 'application/json',
                    data: JSON.stringify(votesData),

                    success: function (returnData) {
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
        };

        self.displayResults = function(data) {
            var groupedVotes = countVotes(data);
            self.drawChart(groupedVotes);
        }
        
        self.initialise = function (pollData) {
            self.pollOptions.initialise(pollData);

            self.maxPerVote(pollData.MaxPerVote);
            self.maxPoints(pollData.MaxPoints);

            resetVote();
        }

        // TODO: Extract chart code from viewModel class - ideally
        // into a shared custom knockout binding to bind to data
        self.drawChart = function (data) {
            //Exit early if data has not changed
            if (chart && JSON.stringify(data) == JSON.stringify(chart.series()[0].data.rawData()))
                return;

            // Hack to fix insight's lack of data reloading
            $('#chart-results').html('');
            var voteData = new insight.DataSet(data);

            chart = new insight.Chart('', '#chart-results')
            .width($("#chart-results").width())
            .height(data.length * 50 + 100);

            var xAxis = new insight.Axis('Votes', insight.scales.linear);
            var yAxis = new insight.Axis('', insight.scales.ordinal)
                .isOrdered(true);
            chart.xAxis(xAxis);
            chart.yAxis(yAxis);

            var series = new insight.RowSeries('votes', voteData, xAxis, yAxis)
            .keyFunction(function (d) {
                return d.Name;
            })
            .valueFunction(function (d) {

                return d.Sum;
            })
            .tooltipFunction(function (d) {
                var voterCount = d.Voters.length;
                var maxToDisplay = 5;
                if (voterCount <= maxToDisplay) {
                    return "Votes: " + d.Sum + "<br />" + d.Voters.toString().replace(/,/g, "<br />");
                }
                else {
                    return "Votes: " + d.Sum + "<br />" + d.Voters.slice(0, maxToDisplay).toString().replace(/,/g, "<br />") + "<br />" + "+ " + (voterCount - maxToDisplay) + " others";
                }
            });

            chart.series([series]);

            chart.draw();
        };

    }

});
