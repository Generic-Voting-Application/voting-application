function ResultViewModel() {
    var self = this;

    self.sessionName = ko.observable();
    self.votes = ko.observableArray();

    self.countVotes = function(voteArray)
    {
        var totalCounts = [];
        voteArray.forEach(function (vote) {
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

    self.drawChart = function(data)
    {
        var voteData = new insight.DataSet(data);

        var chart = new insight.Chart('', '#bar-chart')
            .width(450)
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
            return "Votes: " + d.Count + "<br />" +  d.Voters.toString().replace(/,/g, "<br />");
        });
        chart.series([series]);

        chart.draw();
    }

    self.getOptions = function()
    {
        // Get all options
        $.ajax({
            type: 'GET',
            url: '/api/session/' + self.sessionId + '/vote',

            success: function (data) {
                var groupedVotes = self.countVotes(data);
                self.votes(groupedVotes);
                self.drawChart(groupedVotes);
            }
        });
    }

    self.getSession = function (sessionId) {
        if (sessionId) {
            $.ajax({
                type: 'GET',
                url: "/api/session/" + sessionId,

                success: function (data) {
                    self.sessionName(data.Name);
                }
            });
        }
    }

    $(document).ready(function () {
        self.sessionId = getSessionId();
        self.getSession(self.sessionId);
        self.getOptions();
    });
}

ko.applyBindings(new ResultViewModel());
