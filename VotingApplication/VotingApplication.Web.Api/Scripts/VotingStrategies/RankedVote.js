define(['jquery', 'knockout', 'jqueryUI', 'Common', 'jqueryTouch'], function ($, ko, jqueryUI, Common) {

    return function RankedVote(options) {

        self = this;
        self.options = ko.observableArray(options);
        self.selectedOptions = ko.observableArray();
        self.resultOptions = ko.observableArray();

        var resultsByRound = [];
        var chart;

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

        var sortByBallotCount = function (a, b) {
            return a.ballots.length - b.ballots.length;
        }

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

        var drawChart = function (data, orderedResults) {
            //Exit early if data has not changed
            if (chart && JSON.stringify(data) == JSON.stringify(chart.series()[0].data.rawData()))
                return;

            // Hack to fix insight's lack of data reloading
            $('#results').html('');

            chart = new insight.Chart('', '#results')
            .width($("#results").width())
            .height(orderedResults.length * 40 + 100);

            var orderedNames = orderedResults.map(function (d) {
                var matchingOption = $.grep(self.options(), function (opt) { return opt.Id == d.Id })[0];
                return matchingOption.Name;
            });

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
                .axisRange(-0.1, orderedResults.length);
            chart.xAxis(xAxis);
            chart.yAxis(yAxis);
            chart.legend(new insight.Legend());

            chart.series([]);

            var seriesIndex = 0;

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
                    return d.Voters.toString().replace(/,/g, "<br />");
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
                return 0.5 + orderedResults.length / 2;
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
            drawChart(resultsByRound, orderedResults);
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

        $(document).ready(function () {
            $(".sortable").sortable({
                items: 'tbody > tr',
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
    }

});