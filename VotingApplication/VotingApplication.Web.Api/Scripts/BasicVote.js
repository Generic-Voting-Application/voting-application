define('BasicVote', ['jquery', 'knockout', 'Common', 'PollOptions', 'insight'], function ($, ko, Common, PollOptions, insight) {
    return function BasicVote(pollId, token) {

        var self = this;
        self.pollOptions = new PollOptions(pollId);

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
                    existingOption.Count++;
                    existingOption.Voters.push(voter);
                }
                else {
                    totalCounts.push({
                        Name: optionName,
                        Count: 1,
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
            self.drawChart(groupedVotes);
        };

        self.initialise = function (pollData) {
            self.pollOptions.initialise(pollData);
        };

        // TODO: Extract chart code from viewModel class - ideally
        // into a shared custom knockout binding to bind to data
        self.drawChart = function (data) {
            //Exit early if data has not changed
            if (chart && JSON.stringify(data) === JSON.stringify(chart.series()[0].data.rawData()))
                return;

            // Hack to fix insight's lack of data reloading
            $('#chart-results').html('');
            var voteData = new insight.DataSet(data);

            chart = new insight.Chart('', '#chart-results')
            .width($("#chart-results").width())
            .height(data.length * 50 + 100);

            var xAxis = new insight.Axis('Votes', insight.scales.linear)
                .tickFrequency(1);
            var yAxis = new insight.Axis('', insight.scales.ordinal)
                .isOrdered(true);
            chart.xAxis(xAxis);
            chart.yAxis(yAxis);

            var series = new insight.RowSeries('votes', voteData, xAxis, yAxis)
            .keyFunction(function (d) {
                return d.Name;
            })
            .valueFunction(function (d) {

                return d.Count;
            })
            .tooltipFunction(function (d) {
                var maxToDisplay = 5;
                if (d.Count <= maxToDisplay) {
                    return "Votes: " + d.Count + "<br />" + d.Voters.toString().replace(/,/g, "<br />");
                }
                else {
                    return "Votes: " + d.Count + "<br />" + d.Voters.slice(0, maxToDisplay).toString().replace(/,/g, "<br />") + "<br />" + "+ " + (d.Count - maxToDisplay) + " others";
                }
            });

            if (anonymousPoll) {
                //Prevent tooltip from ever displaying
                series.mouseOver = function () { };
            }

            chart.series([series]);

            chart.draw();
        };
    };

});