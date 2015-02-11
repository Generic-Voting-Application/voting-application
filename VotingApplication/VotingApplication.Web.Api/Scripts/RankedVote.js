define('RankedVote', ['jquery', 'knockout', 'jqueryUI', 'Common', 'PollOptions', 'ResultChart', 'SliderExtension'], function ($, ko, jqueryUI, Common, PollOptions) {

    return function RankedVote(pollId) {

        var self = this;
        self.pollOptions = new PollOptions(pollId);
        self.selectedOptions = ko.observableArray();
        self.remainOptions = ko.observableArray();

        self.chartData = ko.observableArray();
        self.winVotesRequired = ko.observable(0);
        self.winners = ko.observableArray();

        self.roundsRange = ko.observableArray([0, 0]);
        self.roundsDisplay = ko.observableArray([0, 0]);

        self.filteredChartData = ko.computed(function () {
            var display = {
                from: self.roundsDisplay()[0] - 1,
                to: self.roundsDisplay()[1] - 1
            };

            return self.chartData().map(function (option) {
                return {
                    Name: option.Name,
                    Data: option.Data.filter(function (r, i) {
                        return i >= display.from && i <= display.to;
                    })
                };
            });
        });

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
            var ballots = [];
            var totalBallots = 0;
            var resultsByRound = [];

            var options = self.pollOptions.options().map(function (o) { return { Name: o.Name, Id: o.Id, ballots: [] }; });

            // Group votes into ballots (per user)
            votes.forEach(function (vote) {
                if (!ballots[vote.VoterId]) {
                    ballots[vote.VoterId] = [];
                    totalBallots++;
                }
                ballots[vote.VoterId].push(vote);
            });

            // Start counting
            while (options.length > 0) {

                // Clear out all ballots from previous round
                options.forEach(function (d) { d.ballots = []; });
                // Option IDs that haven't been eliminated
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

                // Sort by performance in this round
                options.sort(sortByBallotCount);

                //Convert into a chartable style
                var roundOptions = options.map(function (d) {
                    return {
                        Name: d.Name,
                        Sum: d.ballots.length,
                        Voters: d.ballots.map(function (x) { return x[0].VoterName + " (#" + (x.map(function (y) { return y.OptionId; }).indexOf(d.Id) + 1) + ")"; })
                    };
                });

                // Series representing this round
                if (options[0].ballots.length > 0 || options[options.length - 1].ballots.length > totalBallots / 2) {
                    resultsByRound.push({
                        name: (resultsByRound.length + 1).toString(),
                        data: roundOptions
                    });
                }

                // End if we have a majority
                if (options[options.length - 1].ballots.length > totalBallots / 2) {
                    break;
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
            var tokenGuid = Common.getToken(pollId);

            if (tokenGuid && pollId) {
                // Convert selected options to ranked votes
                var selectedOptionsArray = self.selectedOptions().map(function (option, index) {
                    return {
                        OptionId: option.Id,
                        VoteValue: index + 1,
                        VoterName: Common.getVoterName()
                    };
                });

                $.ajax({
                    type: 'PUT',
                    url: '/api/token/' + tokenGuid + '/poll/' + pollId + '/vote',
                    contentType: 'application/json',
                    data: JSON.stringify(selectedOptionsArray),

                    success: function () {
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
                        data.sort(sortByVoteValue);
                        selectPickedOptions(data);
                    },

                    error: Common.handleError
                });
            }
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

            self.roundsRange([1, 1]);
            self.chartData(groupedVotes);

            // Setup the number of rounds and the display range
            var numRounds = groupedVotes.length ? groupedVotes[0].Data.length : 0;
            self.roundsRange([1, numRounds]);
            self.roundsDisplay([1, numRounds])

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

        // TODO: Extract chart code from viewModel class - ideally
        // into a shared custom knockout binding to bind to data
        self.drawChart = function (data) {
            // Hack to fix insight's lack of data reloading
            $("#chart-results").html('');
            $("#chart-buttons").html('');

            if (!self.chartVisible() || data.length === 0)
                return;

            //Get voter count
            var voterCount = 0;
            data[0].forEach(function (d) {
                voterCount += d.Voters.length;
            });

            chart = new insight.Chart('', '#chart-results')
            .width($("#chart-results").width())
            .height($("#chart-results").width());

            var xAxis = new insight.Axis('', insight.scales.ordinal)
                .isOrdered(true)
                .orderingFunction(function (a, b) {
                    var finalAIndex = orderedNames.indexOf(a.Name);
                    var finalBIndex = orderedNames.indexOf(b.Name);
                    return finalAIndex - finalBIndex;
                })
                .tickLabelRotation(45);

            var yAxis = new insight.Axis('Votes', insight.scales.linear)
                .tickFrequency(1)
                .axisRange(-0.1, voterCount);
            chart.xAxis(xAxis);
            chart.yAxis(yAxis);
            chart.legend(new insight.Legend());

            chart.series([]);

            var seriesIndex = roundIndex;

            //Add a button to display individual rounds
            for (var i = 1; i <= data.length; i++) {
                $("#chart-buttons").append('<button class="btn btn-primary" onclick="self.filterRounds(' + i + ')">Round ' + i + '</button>');
            }

            //Disable the currently selected button
            var currentRoundButton = $("#chart-buttons").children()[roundIndex];
            $(currentRoundButton).attr('disabled', 'disabled');

            //Filter to a specific round
            if (roundIndex > 0) {
                data = data.slice(roundIndex - 1, roundIndex);
                seriesIndex = roundIndex - 1;
            }

            //Map out each round
            data.forEach(function (roundData) {

                var voteData = new insight.DataSet(roundData);
                var series = new insight.ColumnSeries('votes_' + (seriesIndex++), voteData, xAxis, yAxis)
                .keyFunction(function (d) {
                    return d.Name;
                })
                .valueFunction(function (d) {

                    return d.BallotCount;
                })
                .title('Round ' + seriesIndex)
                .tooltipFunction(function (d) {
                    if (d.Voters.length > 0) {
                        return d.Voters.toString().replace(/,/g, "<br />");
                    }
                    else
                        return "Option eliminated";
                });

                var newSeries = chart.series();
                newSeries.push(series);
                chart.series(newSeries);
            });

            //Annotate the decision line
            var series = new insight.MarkerSeries('marker', new insight.DataSet(data[0]), xAxis, yAxis)
            .keyFunction(function (d) {
                return d.Name;
            })
            .valueFunction(function () {
                return voterCount / 2;
            })
            .tooltipFunction(function () {
                return "50% Majority";
            })
            .widthFactor(1.1)
            .thickness(2)
            .title('Majority');

            var newSeries = chart.series();
            newSeries.push(series);
            chart.series(newSeries);

            chart.draw();
        };

        self.toggleChartVisible = function () {
            self.chartVisible(!self.chartVisible());

            //Redraw with all results
            self.filterRounds(0);
        };

        self.filterRounds = function (filterIndex) {
            roundIndex = filterIndex;
            self.drawChart(resultsByRound);
        };

    };
});