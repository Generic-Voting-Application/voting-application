require(['jquery', 'knockout', 'bootstrap', 'insight', 'Common'], function ($, ko, bs, insight, Common) {
    function VoteViewModel() {
        var self = this;

        self.options = ko.observableArray();

        var getOptions = function (pollId) {
            $.ajax({
                type: 'GET',
                url: "/api/session/" + pollId + "/option",

                success: function (data) {
                    self.options(data);
                }
            });
        }

        var getResults = function () {
            $.ajax({
                type: 'GET',
                url: '/api/session/' + self.pollId + '/vote',

                success: function (data) {
                    var groupedVotes = countVotes(data);
                    drawChart(groupedVotes);
                }
            });
        }

        var countVotes = function (votes) {
            var totalCounts = [];
            votes.forEach(function (vote) {
                var optionName = vote.Option.Name;
                var voter = vote.User.Name;

                // Find a vote with the same Option.Name, if it exists.
                var existingOption = totalCounts.filter(function (vote) { return vote.Name == optionName; }).pop();

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
        }

        var drawChart = function (data) {
            var voteData = new insight.DataSet(data);

            if (!self.chart) {
                self.chart = new insight.Chart('', '#bar-chart')
                .width(450)
                .height(data.length * 50 + 100);
            }
            var xAxis = new insight.Axis('Votes', insight.scales.linear)
                .tickFrequency(1);
            var yAxis = new insight.Axis('', insight.scales.ordinal)
                .isOrdered(true);
            self.chart.xAxis(xAxis);
            self.chart.yAxis(yAxis);

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

            self.chart.series([series]);

            self.chart.draw();
        }

        var highlightOption = function (optionId) {
            var optionRows = $("#optionTable > tbody > tr")
            optionRows.removeClass("success");
            var option = self.options().filter(function (d) { return d.Id == optionId }).pop();
            var optionRowIndex = self.options().indexOf(option);

            var matchingRow = optionRows.eq(optionRowIndex);
            matchingRow.addClass("success");
        }

        var getVotes = function () {
            $.ajax({
                type: 'GET',
                url: '/api/user/' + self.userId + '/session/' + self.pollId + '/vote',
                contentType: 'application/json',

                success: function (data) {
                    highlightOption(data[0].OptionId);
                }
            });
        }

        var showSection = function (element) {
            var siblings = element.siblings();
            for (var i = 0; i < siblings.length; i++) {
                $(siblings[i]).collapseSection('hide');
            }
            element.collapseSection('show')
        }

        self.showSection = function (data, event) {
            showSection($(data).parent());
        }

        self.submitLogin = function (data, event) {
            $.ajax({
                type: 'PUT',
                url: '/api/user',
                contentType: 'application/json',
                data: JSON.stringify({
                    Name: $("#loginUsername").val()
                }),

                success: function (data) {
                    //Expire in 6 hours
                    var expiryTime = Date.now() + (6 * 60 * 60 * 1000)
                    self.userId = data;
                    localStorage["userId"] = JSON.stringify({ id: self.userId, expires: expiryTime });
                    showSection($('#voteSection'));
                },

                error: function (jqXHR, textStatus, errorThrown) {
                    if (jqXHR.status == 400) {
                        $('#loginSection').addClass("has-error");
                        $('#usernameWarnMessage').show();
                        
                    }
                }
            });
        }

        self.doVote = function (data, event) {
            if (self.userId && self.pollId) {
                $.ajax({
                    type: 'PUT',
                    url: '/api/user/' + self.userId + '/vote',
                    contentType: 'application/json',
                    data: JSON.stringify({
                        OptionId: data.Id,
                        SessionId: self.pollId
                    }),

                    success: function (returnData) {
                        var currentRow = event.currentTarget.parentElement.parentElement;
                        showSection($('#resultSection'));
                    }
                });
            }
        }

        $('#voteSection .accordion-body').on('show.bs.collapse', function () {
            getVotes();
        });

        $('#resultSection .accordion-body').on('show.bs.collapse', function () {
            getResults();
        });

        $(document).ready(function () {
            self.pollId = Common.getPollId()
            self.userId = Common.currentUserId();

            getOptions(self.pollId);

            if (self.userId) {
                showSection($('#voteSection'));
            } else {
                showSection($('#loginSection'));
            }
        });
    }

    ko.applyBindings(new VoteViewModel());
});

