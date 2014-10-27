function ResultViewModel() {
    var self = this;

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
            .height(400);
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

    $(document).ready(function () {
        // Get all options
        $.ajax({
            type: 'GET',
            url: "/api/vote",

            success: function (data) {
                var groupedVotes = self.countVotes(data);

                groupedVotes.forEach(function (vote) {
                    self.votes.push(vote);
                });

                self.drawChart(groupedVotes);
            }
        });
    });
}

ko.applyBindings(new ResultViewModel());
