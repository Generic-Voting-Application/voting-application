require(['jquery', 'knockout', 'bootstrap', 'insight', 'Common'], function ($, ko, bs, insight, Common) {
    function VoteViewModel() {
        var self = this;

        self.options = ko.observableArray();
        self.pollName = ko.observable("Poll Name");
        self.pollCreator = ko.observable("Poll Creator");

        var getPollDetails = function (pollId) {
            $.ajax({
                type: 'GET',
                url: "/api/session/" + pollId,

                success: function (data) {
                    self.pollName(data.Name);
                    self.pollCreator(data.Creator);
                    self.options(data.Options);
                }
            });
        };

        var getResults = function () {
            $.ajax({
                type: 'GET',
                url: '/api/session/' + self.pollId + '/vote',

                success: function (data) {
                    var groupedVotes = countVotes(data);
                    drawChart(groupedVotes);
                }
            });
        };

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

            chart.series([series]);

            chart.draw();
        };

        var clearOptionHighlighting = function () {
            $("#optionTable > tbody > tr").removeClass("success");
        };

        var highlightOption = function (optionId) {

            clearOptionHighlighting();

            var optionRows = $("#optionTable > tbody > tr");
            var option = self.options().filter(function (d) { return d.Id == optionId; }).pop();
            var optionRowIndex = self.options().indexOf(option);

            var matchingRow = optionRows.eq(optionRowIndex);
            matchingRow.addClass("success");
        };

        var getVotes = function () {
            $.ajax({
                type: 'GET',
                url: '/api/user/' + self.userId + '/session/' + self.pollId + '/vote',
                contentType: 'application/json',

                success: function (data) {
                    if (data[0]) {
                        highlightOption(data[0].OptionId);
                    }
                    else {
                        clearOptionHighlighting();
                    }
                }
            });
        };

        var showSection = function (element) {
            var siblings = element.siblings();
            for (var i = 0; i < siblings.length; i++) {
                $(siblings[i]).collapseSection('hide');
                $(siblings[i]).removeClass('panel-primary');

            }
            element.collapseSection('show');
            element.addClass('panel-primary');
        };

        self.showSection = function (data, event) {
            showSection($(data).parent());
        };

        self.submitLogin = function (data, event) {
            var username = $("#loginUsername").val();
            $.ajax({
                type: 'PUT',
                url: '/api/user',
                contentType: 'application/json',
                data: JSON.stringify({
                    Name: username
                }),

                success: function (data) {
                    Common.loginUser(data, username);
                    self.userId = data;
                    showSection($('#voteSection'));
                },

                error: function (jqXHR, textStatus, errorThrown) {
                    if (jqXHR.status == 400) {
                        $('#loginSection').addClass("has-error");
                        $('#usernameWarnMessage').show();

                    }
                }
            });
        };

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
        };

        $('#voteSection .accordion-body').on('show.bs.collapse', function () {
            getVotes();
        });

        $('#resultSection .accordion-body').on('show.bs.collapse', function () {
            getResults();
        });

        $(document).ready(function () {
            self.pollId = Common.getPollId();
            self.userId = Common.currentUserId();

            getPollDetails(self.pollId);

            if (self.userId) {
                $('#loginUsername').val(Common.currentUserName());
                showSection($('#voteSection'));
            } else {
                showSection($('#loginSection'));
            }
        });
    }

    ko.applyBindings(new VoteViewModel());
});
