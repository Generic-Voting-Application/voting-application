(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.controller('ResultsPageController', ['$scope', '$location', 'PollAction', function ($scope, $location, PollAction) {
        var chart;

        // Turn "/#/results/abc/123" into "/#/voting/abc/123"
        var locationTokens = $location.url().split("/");
        locationTokens.splice(0, 2);
        $scope.votingLink = '/#/voting/' + locationTokens.join("/");

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
            var chartHeight = Math.min(barCount * 50 + 100, 600);

            chart = new insight.Chart('', '#results-chart')
                .width(350)
                .height(chartHeight);

            var voteAxis = new insight.Axis('Votes', insight.scales.linear);
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
                    .title('Results');
                    /*
                    .tooltipFunction(function (d) {
                        var voterCount = d.Voters.length;
                        var votersDisplay = d.Voters;
                        var addition = "";

                        var maxToDisplay = 5;
                        if (voterCount > maxToDisplay) {
                            votersDisplay = d.Voters.slice(0, maxToDisplay);
                            addition = "<br />+ " + (voterCount - maxToDisplay) + " others";
                        }

                        return (series.Name ? series.Name + "<br />" : "")
                            + "Votes: " + d.Sum + "<br />" + votersDisplay.toString().replace(/,/g, "<br />") + addition;
                    });
                    */

            chart.series([allSeries]);

            // First parameter disables animation
            chart.draw(true);
        }

        $scope.reloadData = function () {
            PollAction.getResults(PollAction.currentPollId(), function (data) {
                var groupedData = {};

                // Group together votes for the same options
                data.map(function (d) {
                    if (!(d.OptionName in groupedData)) {
                        groupedData[d.OptionName] = 0;
                    }

                    groupedData[d.OptionName] += d.VoteValue;
                });

                var datapoints = [];
                // Separate into datapoints
                for (var key in groupedData) {
                    datapoints.push({ Name: key, Sum: groupedData[key] });
                }

                drawChart(datapoints);
            });
        }

        $scope.reloadData();
    }]);
})();
