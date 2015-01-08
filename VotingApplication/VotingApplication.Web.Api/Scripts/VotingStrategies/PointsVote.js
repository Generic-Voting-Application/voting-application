define(['jquery', 'knockout', 'Common'], function ($, ko, Common) {

    return function PointsVote(pollId, token) {

        self = this;
        self.options = ko.observableArray();
        self.maxPerVote = ko.observable();
        self.maxPoints = ko.observable();
        self.optionAdding = ko.observable();
        self.pointsArray = ko.observableArray();

        var chart;

        self.pointsRemaining = ko.computed(function () {
            var total = 0;
            ko.utils.arrayForEach(self.pointsArray(), function (item) {
                total += item;
            });
            var remaining = self.maxPoints() - total;
            return remaining;
        });

        self.percentRemaining = ko.computed(function () {
            return (self.pointsRemaining() / self.maxPoints()) * 100;
        });

        var resetVote = function () {
            //Populate with an array of options.length number of 0-values
            self.pointsArray(Array.apply(null, Array(self.options().length)).map(Boolean).map(Number));
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
                var voter = "Anonymous User";
                var voteValue = vote.PollValue;

                if (vote.User) {
                    voter = vote.User.Name;
                }

                var voteString = voter + " (" + voteValue + ")";

                // Find a vote with the same Option.Name, if it exists.
                var existingOption = totalCounts.filter(function (vote) { return vote.Name == optionName; }).pop();

                if (existingOption) {
                    existingOption.Sum += vote.PollValue;
                    existingOption.Voters.push(voteString);
                }
                else {
                    totalCounts.push({
                        Name: optionName,
                        Sum: vote.PollValue,
                        Voters: [voteString]
                    });
                }
            });
            return totalCounts;
        };

        var drawChart = function (data) {
            //Exit early if data has not changed
            if (chart && JSON.stringify(data) == JSON.stringify(chart.series()[0].data.rawData()))
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

        var refreshOptions = function () {
            $.ajax({
                type: 'GET',
                url: "/api/poll/" + pollData.UUID + "/option",

                success: function (data) {
                    var newOptionCount = data.length - self.options().length;
                    self.options.removeAll();
                    self.options(data);

                    for (var i = 0; i < newOptionCount; i++){
                        self.pointsArray.push(0);
                    }

                    updateAllButtons();
                }
            });
        }

        var updateButtons = function (buttonGroup) {
            var index = $("#optionTable span").index(buttonGroup);
            var minusButton = $(buttonGroup).find(".btn.pull-left");

            if (self.pointsArray().length > 0) {
                var sumOfAllPoints = self.pointsArray().reduce(function (prevValue, currentValue) { return prevValue + currentValue; });
                var pointsForGroup = self.pointsArray()[index];
            }


            // Minus button clickable for value > 0
            if (pointsForGroup > 0) {
                minusButton.removeAttr('disabled');
            }
            else {
                minusButton.attr('disabled', 'disabled');
            }

            // Plus button clickable if more points can be added to group and total
            if (sumOfAllPoints >= self.maxPoints()) {
                $("#optionTable span .btn.pull-right").attr('disabled', 'disabled');
            }
            else {
                var $allPlusButtons = $("#optionTable span .btn.pull-right");
                for (var i = 0; i < $allPlusButtons.length; i++) {
                    var plusButton = $allPlusButtons[i];
                    if (self.pointsArray()[i] >= self.maxPerVote()) {
                        $(plusButton).attr('disabled', 'disabled');
                    }
                    else {
                        $(plusButton).removeAttr('disabled');
                    }
                }
            }
        }

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

        self.doVote = function (data, event) {
            var userId = Common.currentUserId(pollId);

            var votesData = [];

            for (var i = 0; i < self.options().length; i++) {
                if (self.pointsArray()[i] == 0) {
                    continue;
                }

                var vote = {
                    OptionId: self.options()[i].Id,
                    PollId: pollId,
                    PollValue: self.pointsArray()[i],
                    Token: { TokenGuid: token }
                };

                votesData.push(vote);
            }

            if (userId && pollId) {
                $.ajax({
                    type: 'PUT',
                    url: '/api/user/' + userId + '/poll/' + pollId + '/vote',
                    contentType: 'application/json',
                    data: JSON.stringify(votesData),

                    success: function (returnData) {
                        $('#resultSection > div')[0].click();
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
                    var allOptions = self.options();
                    for (var i = 0; i < data.length; i++) {
                        //Find index of previously voted option
                        var vote = allOptions.filter(function (d) {
                            return d.Id == data[i].OptionId;
                        })[0];
                        var optionIndex = self.options().indexOf(vote);

                        if (optionIndex == -1)
                            continue;

                        self.pointsArray()[optionIndex] = data[i].PollValue;
                        self.pointsArray.valueHasMutated();
                    }
                    updateAllButtons();
                }
            });
        };

        self.displayResults = function(data) {
            var groupedVotes = countVotes(data);
            drawChart(groupedVotes);
        }

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
                url: '/api/poll/' + Common.getPollId() + '/option',
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

        self.initialise = function (pollData) {

            self.options(pollData.Options);
            self.maxPerVote(pollData.MaxPerVote);
            self.maxPoints(pollData.MaxPoints);
            self.optionAdding(pollData.OptionAdding);

            resetVote();
        }

        
    }

});