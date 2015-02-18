(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.controller('ResultsPageController', ['$scope', '$location', 'PollAction', function ($scope, $location, PollAction) {
        var chart;

        // Turn "/#/results/abc/123" into "/#/voting/abc/123"
        var locationTokens = $location.url().split("/");
        locationTokens.splice(0, 2);
        $scope.votingLink = '/#/voting/' + locationTokens.join("/");

        $scope.winner = 'Lorem';
        //Whether or not we have an "s" on the end of "Winner"
        $scope.plural = '';

        var drawChart = function (data) {
            if (!data.length) return;

            var dataUnchanged = chart && data.length === chart.series().length;

            var barCount = 0;
            for (var n = 0; n < data.length; n++) {
                barCount++;
                var chartSeries = chart ? chart.series() : null;
                dataUnchanged = dataUnchanged &&
                    chart && chart.series().length > n &&
                    JSON.stringify(data[n].Data) == JSON.stringify(chart.series()[n].data.rawData());
            }
            //Exit early if data has not changed
            if (dataUnchanged)
                return;

            // Hack to fix insight's lack of data reloading
            //$element.html('');

            // Fixed height for column chart, but scale to number of rows for bar charts
            var chartHeight = Math.min(data.length * 50 + 100, 600);

            chart = new insight.Chart('', '#results-chart')
                .width(Math.min(600, window.innerWidth - 60))
                .height(chartHeight);

            var voteAxis = new insight.Axis('', insight.scales.linear);
            var optionAxis = new insight.Axis('', insight.scales.ordinal)
                    .isOrdered(true);

            var xAxis = voteAxis;
            var yAxis = optionAxis;

            chart.xAxis(xAxis);
            chart.yAxis(yAxis);

            chart.autoMargin(true);

            var allSeries = new insight.RowSeries('Results', new insight.DataSet(data), xAxis, yAxis)
                    .keyFunction(function (d) {
                        return d.Name;
                    })
                    .valueFunction(function (d) {
                        return d.Sum;
                    })
                    .title('Results')
                    .tooltipFunction(function (d) {
                        var voterCount = d.Voters.length;
                        var votersDisplay = d.Voters;
                        var addition = "";

                        var maxToDisplay = 5;
                        if (voterCount > maxToDisplay) {
                            votersDisplay = d.Voters.slice(0, maxToDisplay);
                            addition = "<br />+ " + (voterCount - maxToDisplay) + " others";
                        }

                        return "<b>" + d.Name + "</b>: " + d.Sum + " votes<br/><br/>" + votersDisplay.join("<br />") + addition;
                    });

            chart.series([allSeries]);

            // First parameter disables animation
            chart.draw(true);
        }

        $scope.reloadData = function () {
            PollAction.getResults(PollAction.currentPollId(), function (data) {
                var groupedData = {};

                // Group together votes for the same options
                data.forEach(function (d) {
                    if (!(d.OptionName in groupedData)) {
                        groupedData[d.OptionName] = { Value: 0, Voters: [] }
                    }

                    groupedData[d.OptionName].Value += d.VoteValue;
                    groupedData[d.OptionName].Voters.push(d.VoterName);

                });

                var winningScore = 0;

                var datapoints = [];
                // Separate into datapoints
                for (var key in groupedData) {
                    datapoints.push({ Name: key, Sum: groupedData[key].Value, Voters: groupedData[key].Voters });
                    winningScore = Math.max(winningScore, groupedData[key].Value);
                }

                var winners = datapoints.filter(function (d) {
                    return d.Sum === winningScore;
                });

                $scope.winner = winners.map(function (d) {
                    return d.Name;
                }).join(", ");

                $scope.plural = (winners.length > 1) ? 's (Draw)' : '';

                drawChart(datapoints);
            });
        }

        $scope.reloadData();
    }]);
})();
