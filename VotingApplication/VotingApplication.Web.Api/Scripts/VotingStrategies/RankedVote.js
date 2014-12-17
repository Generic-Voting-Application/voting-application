define(['jquery', 'knockout', 'jqueryUI', 'Common', 'jqueryTouch'], function ($, ko, jqueryUI, Common) {

    return function RankedVote(options, pollData) {

        self = this;
        self.options = ko.observableArray(options);
        self.selectedOptions = ko.observableArray();
        self.resultOptions = ko.observableArray();
        self.optionAdding = ko.observable(pollData.OptionAdding);
        self.chartVisible = ko.observable(false);

        var resultsByRound = [];
        var orderedNames = [];
        var chart;
        var roundIndex = 0;

        var selectOption = function (option) {
            var $option = $('#optionTable > tbody > tr').filter(function () {
                return $(this).attr('data-id') == option.Id;
            });

            $('#selectionTable > tbody').append($option.remove());
            self.selectedOptions.push(option)
        }

        var selectPickedOptions = function (pickedOptions) {
            self.selectedOptions.removeAll();

            pickedOptions.forEach(function (option) {
                var pickedOption = ko.utils.arrayFirst(self.options(), function (item) {
                    return item.Id == option.OptionId;
                });

                selectOption(pickedOption);
            });
        }

        var resetOptions = function () {
            $('#optionTable > tbody').append($('#selectionTable > tbody').remove('tr').children());
        }
        
        var sortByPollValue = function (a, b) {
            return a.PollValue - b.PollValue;
        }

        var refreshOptions = function () {
            $.ajax({
                type: 'GET',
                url: "/api/poll/" + pollData.UUID + "/option",

                success: function (data) {

                    data.forEach(function (dataOption) {
                        if (self.options().filter(function (option) { return option.Id == dataOption.Id }).length > 0) {
                            return;
                        }
                        if (self.selectedOptions().filter(function (option) { return option.Id == dataOption.Id }).length > 0) {
                            return;
                        }

                        self.options.push(dataOption);
                    });
                }
            });
        };

        var sortByBallotCount = function (a, b) {
            return a.ballots.length - b.ballots.length;
        };

        var countVotes = function (votes) {
            var options = [];
            var orderedOptions = [];
            var ballots = [];
            var totalBallots = 0;
            var totalOptions = self.options().length;
            resultsByRound = [];

            for (var k = 0; k < totalOptions; k++) {
                var optionId = self.options()[k].Id;
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
                    ballot.sort(sortByPollValue);
                    var availableChoices = ballot.filter(function (option) { return $.inArray(option.OptionId, availableOptions) != -1; })
                    if (availableChoices.length > 0) {
                        var firstChoiceId = availableChoices[0].OptionId
                        var firstChoiceOption = options.filter(function (option) { return option.Id == firstChoiceId })[0];
                        firstChoiceOption.ballots.push(ballot)
                    }
                });

                options.sort(sortByBallotCount);

                //Convert into a chartable style
                var roundOptions = options.map(function (d) {
                    var matchingOption = $.grep(self.options(), function (opt) { return opt.Id == d.Id })[0];
                    return {
                        Name: matchingOption.Name,
                        BallotCount: d.ballots.length,
                        Voters: d.ballots.map(function (x) { return x[0].User.Name + " (#" + (x.map(function (y) { return y.OptionId }).indexOf(matchingOption.Id) + 1) + ")"; })
                    }
                });

                //Add in removed options as 0-value
                orderedOptions.forEach(function (d) {
                    var matchingOption = $.grep(self.options(), function (opt) { return opt.Id == d.Id })[0];
                    roundOptions.push({
                        Name: matchingOption.Name,
                        BallotCount: 0,
                        Voters: []
                    });
                })

                if (options[0].ballots.length > 0) {
                    resultsByRound.push(roundOptions);
                }

                // End if we have a majority
                if (options[options.length - 1].ballots.length > totalBallots / 2) {
                    break;
                }

                // Remove all last place options
                var lastPlaceOption = options[0];
                var lastPlaceBallotCount = lastPlaceOption.ballots.length;

                var removedOptions = options.filter(function (d) { return d.ballots.length == lastPlaceBallotCount; });
                // Track at what point an option was removed from the running
                removedOptions.map(function (d) { d.rank = options.length - removedOptions.length + 1; return d; });
                orderedOptions.push.apply(orderedOptions, removedOptions);

                options = options.filter(function (d) { return d.ballots.length > lastPlaceBallotCount; });
            }

            orderedOptions.push.apply(orderedOptions, options);
            orderedOptions.reverse();

            return orderedOptions;
        }

        var drawChart = function (data) {
            // Hack to fix insight's lack of data reloading
            $("#chart-results").html('');
            $("#chart-buttons").html('');

            if (!self.chartVisible())
                return;

            chart = new insight.Chart('', '#chart-results')
            .width($("#chart-results").width())
            .height(orderedNames.length * 40 + 100);

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
                .axisRange(-0.1, orderedNames.length);
            chart.xAxis(xAxis);
            chart.yAxis(yAxis);
            chart.legend(new insight.Legend());

            chart.series([]);

            var seriesIndex = roundIndex;

            //Add a button to display individual rounds
            var button = $("#chart-buttons").append('<button class="btn btn-primary" onclick="self.filterRounds(0)">All Rounds</button>');
            
            for (var i = 1; i <= data.length; i++)
            {
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

                var newSeries = chart.series()
                newSeries.push(series);
                chart.series(newSeries);
            });

            //Annotate the decision line
            var series = new insight.MarkerSeries('marker', new insight.DataSet(orderedNames), xAxis, yAxis)
            .keyFunction(function (d) {
                return d;
            })
            .valueFunction(function (d) {
                return 0.5 + orderedNames.length / 2;
            })
            .tooltipFunction(function (d) {
                return "50% Majority";
            })
            .widthFactor(1.1)
            .thickness(2)
            .title('Majority');

            var newSeries = chart.series()
            newSeries.push(series);
            chart.series(newSeries);

            chart.draw();
        };

        var displayResults = function (votes) {
            var orderedResults = countVotes(votes);

            orderedNames = orderedResults.map(function (d) {
                var matchingOption = $.grep(self.options(), function (opt) { return opt.Id == d.Id })[0];
                return matchingOption.Name;
            });

            //Exit early if data has not changed
            if (chart && JSON.stringify(resultsByRound) == JSON.stringify(chart.series().slice(0, chart.series().length - 1).map(function (d) { return d.data.rawData() })))
                return;

            // Fill in the table
            self.resultOptions.removeAll();
            for (var i = 0; i < orderedResults.length; i++) {

                var option = ko.utils.arrayFirst(self.options(), function (item) {
                    return item.Id == orderedResults[i].Id;
                });
                option.Rank = orderedResults[i].rank || 1;
                self.resultOptions.push(option);
            }

            drawChart(resultsByRound.slice(0));
        }

        self.filterRounds = function (filterIndex) {
            roundIndex = filterIndex;
            drawChart(resultsByRound);
        }

        self.doVote = function (data, event) {
            var userId = Common.currentUserId();
            var pollId = Common.getPollId();
            var token = Common.getToken();

            if (userId && pollId) {

                var selectionRows = $('#selectionTable > tbody > tr');

                var selectedOptionsArray = [];
                var minRank = Number.MAX_VALUE;
                ko.utils.arrayForEach(self.selectedOptions(), function (selection) {

                    var $optionElement = selectionRows.filter(function () {
                        return $(this).attr('data-id') == selection.Id;
                    });

                    var rank = $("#selectionTable > tbody > tr").index($optionElement[0]);
                    minRank = Math.min(minRank, rank);

                    selectedOptionsArray.push({
                        OptionId: selection.Id,
                        PollId: pollId,
                        PollValue: rank,
                        Token: { TokenGuid: token }
                    });
                });

                // Offset by the first value to account for table headers and sort out 0 index
                selectedOptionsArray.map(function (option) {
                    option.PollValue -= minRank - 1;
                });

                $.ajax({
                    type: 'PUT',
                    url: '/api/user/' + userId + '/vote',
                    contentType: 'application/json',
                    data: JSON.stringify(selectedOptionsArray),

                    success: function (returnData) {
                        $('#resultSection > div')[0].click();
                    }
                });
            }
        };

        self.getVotes = function (pollId, userId) {
            resetOptions();

            $.ajax({
                type: 'GET',
                url: '/api/user/' + userId + '/poll/' + pollId + '/vote',
                contentType: 'application/json',

                success: function (data) {
                    data.sort(sortByPollValue);
                    selectPickedOptions(data);
                }
            });
        };

        self.getResults = function (pollId) {

            $.ajax({
                type: 'GET',
                url: '/api/poll/' + pollId + '/vote',

                success: function (data) {
                    displayResults(data);
                }
            });
        };

        self.addOption = function () {
            //Don't submit without an entry in the name field
            if ($("#newName").val() === "") {
                return;
            }

            var newName = $("#newName").val();
            var newInfo = $("#newInfo").val();
            var newDescription = $("#newDescription").val();

            //Reset before posting, to prevent double posts.
            $("#newName").val("");
            $("#newDescription").val("");
            $("#newInfo").val("");

            $.ajax({
                type: 'POST',
                url: '/api/poll/' + pollData.UUID + '/option',
                contentType: 'application/json',

                data: JSON.stringify({
                    Name: newName,
                    Description: newDescription,
                    Info: newInfo
                }),

                success: function () {
                    refreshOptions();
                }
            });
        };

        $("#newOptionRow").keypress(function (event) { Common.keyIsEnter(event, self.addOption); });
        self.toggleChartVisible = function () {
            self.chartVisible(!self.chartVisible());

            //Redraw with all results
            self.filterRounds(0);
        }

        $(document).ready(function () {
            $(".sortable").sortable({
                items: 'tbody > tr:not(#newOptionRow)',
                connectWith: '.sortable',
                axis: 'y',
                dropOnEmpty: true,
                receive: function (e, ui) {
                    var itemId = ui.item.attr('data-id');
                    var item = ko.utils.arrayFirst(self.options(), function (item) {
                        return item.Id == itemId;
                    });

                    var rows = $(this).find('tbody > tr');
                    var rowIds = $.map(rows, function (d, i) { return $(d).data("id") });
                    var insertionIndex = rowIds.indexOf(item.Id);

                    if ($(e.target).hasClass('selection-content')) {
                        //Insert at index in selectedOptions
                        self.selectedOptions.splice(insertionIndex, 0, item);
                    } else {
                        //Get index of where we dropped the item
                        self.selectedOptions.remove(item);
                    }

                    //Make sure rows are a part of the table body
                    if (rows.length == 0) {
                        $(this).find('tbody').append(ui.item)
                    }
                    else {
                        rows.eq(insertionIndex).before(ui.item);
                    }

                }
            });
        });

        $.ajax({
            type: 'GET',
            url: '/Partials/VotingStrategies/RankedVoteResults.html',
            dataType: 'html',

            success: function (data) {
                $("#results").html(data);
                ko.applyBindings(self, $('#results')[0]);
            }
        });
    }

});