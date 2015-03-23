/// <reference path="../Services/IdentityService.js" />
/// <reference path="../Services/PollService.js" />
/// <reference path="../Services/VoteService.js" />
(function () {
    angular
        .module('GVA.Voting')
        .controller('ResultsPageController', ResultsPageController);

    ResultsPageController.$inject = ['$scope', '$routeParams', 'IdentityService', 'VoteService', 'RoutingService'];

    function ResultsPageController($scope, $routeParams, IdentityService, VoteService, RoutingService) {

        var pollId = $routeParams.pollId;
        var tokenId = $routeParams['tokenId'] || '';
        var chart;

        // Turn "/#/results/abc/123" into "/#/voting/abc/123"
        $scope.votingLink = RoutingService.getVotePageUrl(pollId, tokenId);

        $scope.winner = 'Lorem';
        //Whether or not we have an "s" on the end of "Winner"
        $scope.plural = '';

        $scope.voteCount = 0;

        var drawChart = function (data) {

            // Hack to fix insight's lack of data reloading
            document.getElementById('results-chart').innerHTML = '';

            if (!data.length) {
                return;
            }

            var highestDataValue = data.reduce(function (prev, curr) {
                return curr.Sum > prev.Sum ? curr : prev;
            }).Sum;

            var tickFrequency = Math.max((Math.pow(10, (Math.round(Math.log(highestDataValue) / Math.log(10)) - 1))), 1);

            var xAxis = new insight
                .Axis('', insight.scales.linear)
                .tickFrequency(tickFrequency);

            var yAxis = new insight
                .Axis('', insight.scales.ordinal)
                .isOrdered(true);

            var dataUnchanged = chart && data.length === chart.series().length;

            var barCount = 0;
            for (var n = 0; n < data.length; n++) {
                barCount++;
                dataUnchanged = dataUnchanged &&
                    chart &&
                    chart.series().length > n &&
                    JSON.stringify(data[n].Data) === JSON.stringify(chart.series()[n].data.rawData());
            }

            //Exit early if data has not changed
            if (dataUnchanged) {
                return;
            }

            // Fixed height for column chart, but scale to number of rows for bar charts
            var chartHeight = Math.min(data.length * 50 + 100, 600);
            var chartWidth = Math.min(600, document.getElementById('results-chart').offsetWidth);

            chart = new insight
                .Chart('', '#results-chart')
                .width(chartWidth)
                .height(chartHeight);

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
                    var addition = '';

                    var maxToDisplay = 5;
                    if (voterCount > maxToDisplay) {
                        votersDisplay = d.Voters.slice(0, maxToDisplay);
                        addition = '<br />+ ' + (voterCount - maxToDisplay) + ' others';
                    }

                    return '<b>' + d.Name + '</b>: ' + d.Sum + ' votes<br/><br/>' + votersDisplay.join('<br />') + addition;
                });

            chart.series([allSeries]);

            // First parameter disables animation
            chart.draw(true);
        };

        var reloadData = function () {
            VoteService.getResults(pollId, getResultsSuccessCallback);
        };

        function getResultsSuccessCallback(data, status) {

            if (data) {
                $scope.voteCount = data.length;
            }

            var groupedData = {};

            // Group together votes for the same options
            data.forEach(function (d) {
                if (!(d.OptionName in groupedData)) {
                    groupedData[d.OptionName] = { Value: 0, Voters: [] };
                }

                groupedData[d.OptionName].Value += d.VoteValue;
                groupedData[d.OptionName].Voters.push(d.VoterName);

            });

            var winningScore = 0;

            var datapoints = [];

            // Separate into datapoints
            for (var key in groupedData) {
                if (groupedData.hasOwnProperty(key)) {
                    datapoints.push({ Name: key, Sum: groupedData[key].Value, Voters: groupedData[key].Voters });
                    winningScore = Math.max(winningScore, groupedData[key].Value);
                }
            }

            var winners = datapoints.filter(function (d) {
                return d.Sum === winningScore;
            });

            $scope.winner = winners.map(function (d) {
                return d.Name;
            }).join(', ');

            $scope.plural = (winners.length > 1) ? 's (Draw)' : '';

            drawChart(datapoints);
        }

        VoteService.refreshLastChecked(pollId);
        reloadData();
        setInterval(reloadData, 3000);
    }
})();
