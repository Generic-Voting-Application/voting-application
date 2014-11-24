define(['jquery', 'knockout', 'Common'], function ($, ko, Common) {


    return function PointsVote(options) {

        self = this;
        self.options = ko.observableArray(options);
        self.pointsArray = ko.observableArray();

        var maxPoints = 7;
        var maxPerVote = 3;
        
        var resetVote = function () {
            //Populate with an array of options.length number of 0-values
            self.pointsArray(Array.apply(null, Array(options.length)).map(Boolean).map(Number));
            updateAllButtons();
        };

        var updateAllButtons = function () {
            var allButtonGroups = $("#optionTable span");
            for (var i = 0; i < allButtonGroups.length; i++) {
                var buttonGroup = allButtonGroups[i];
                updateButtons(buttonGroup);
            }
        }

        var countVotes = function (votes) {
            var totalCounts = [];
            votes.forEach(function (vote) {
                var optionName = vote.Option.Name;
                var voter = vote.User.Name;

                // Find a vote with the same Option.Name, if it exists.
                var existingOption = totalCounts.filter(function (vote) { return vote.Name == optionName; }).pop();

                if (existingOption) {
                    existingOption.Sum += vote.Value;
                    existingOption.Voters.push(voter);
                }
                else {
                    totalCounts.push({
                        Name: optionName,
                        Sum: vote.Value,
                        Voters: [voter]
                    });
                }
            });
            return totalCounts;
        };

        var drawChart = function (data) {
            // Hack to fix insight's lack of data reloading
            $('#bar-chart').html('');
            var voteData = new insight.DataSet(data);

            var chart = new insight.Chart('', '#bar-chart')
            .width($("#bar-chart").width())
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

        self.decreaseVote = function (data, event) {
            var pointsIndex = self.options().indexOf(data);
            self.pointsArray()[pointsIndex]--;
            self.pointsArray.valueHasMutated();

            updateButtons(event.target.parentElement);
        }

        self.increaseVote = function (data, event) {
            var pointsIndex = self.options().indexOf(data);
            self.pointsArray()[pointsIndex]++;
            self.pointsArray.valueHasMutated();

            updateButtons(event.target.parentElement);
        }

        var updateButtons = function (buttonGroup) {
            var index = $("#optionTable span").index(buttonGroup);
            var minusButton = $(buttonGroup).find(".btn.pull-left");

            var sumOfAllPoints = self.pointsArray().reduce(function(prevValue, currentValue) { return prevValue + currentValue; });
            var pointsForGroup = self.pointsArray()[index];

            // Minus button clickable for value > 0
            if (pointsForGroup > 0) {
                minusButton.removeAttr('disabled');
            }
            else {
                minusButton.attr('disabled', 'disabled');
            }

            // Plus button clickable if more points can be added to group and total
            if (sumOfAllPoints >= maxPoints) {
                $("#optionTable span .btn.pull-right").attr('disabled', 'disabled');
            }
            else  {
                var $allPlusButtons = $("#optionTable span .btn.pull-right");
                for (var i = 0; i < $allPlusButtons.length; i++) {
                    var plusButton = $allPlusButtons[i];
                    if (self.pointsArray()[i] >= maxPerVote) {
                        $(plusButton).attr('disabled', 'disabled');
                    }
                    else {
                        $(plusButton).removeAttr('disabled');
                    }
                }
            }
        }

        self.doVote = function (data, event) {
            var userId = Common.currentUserId();
            var pollId = Common.getPollId();

            var votesData = [];

            for (var i = 0; i < self.options().length; i++) {
                if (self.pointsArray()[i] == 0) {
                    continue;
                }

                var vote = {
                    OptionId: self.options()[i].Id,
                    SessionId: pollId,
                    Value: self.pointsArray()[i]
                };

                votesData.push(vote);
            }

            if (userId && pollId) {
                $.ajax({
                    type: 'PUT',
                    url: '/api/user/' + userId + '/vote',
                    contentType: 'application/json',
                    data: JSON.stringify(votesData),

                    success: function (returnData) {
                        $('#resultSection > div')[0].click();
                    }
                });
            }
        };

        self.getVotes = function (pollId, userId) {
            $.ajax({
                type: 'GET',
                url: '/api/user/' + userId + '/session/' + pollId + '/vote',
                contentType: 'application/json',

                success: function (data) {
                    resetVote();
                    var allOptions = self.options();
                    for (var i = 0; i < data.length; i++) {
                        //Find index of previously voted option
                        var vote = allOptions.filter(function (d) {
                            return d.Id == data[i].OptionId;
                        })[0];
                        var optionIndex = self.options().indexOf(vote);

                        if (optionIndex == -1)
                            continue;

                        self.pointsArray()[optionIndex] = data[i].Value;
                        self.pointsArray.valueHasMutated();
                    }
                    updateAllButtons();
                }
            });
        };

        self.getResults = function (pollId) {
            $.ajax({
                type: 'GET',
                url: '/api/session/' + pollId + '/vote',

                success: function (data) {
                    var groupedVotes = countVotes(data);
                    drawChart(groupedVotes);
                }
            });
        };

        resetVote();
    }

});