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

        var canvas = document.createElement('canvas');

        var textWidth = function (text) {
            var context = canvas.getContext('2d');
            context.font = '16px Open Sans';
            var metrics = context.measureText(text);
            return metrics.width;
        };

        var tickFrequencyForRange = function (range) {
            var tickFrequency = 1;
            var step = 0;

            while (range / tickFrequency >= 10) {
                // Multiply by: 2x, 5x, 10x, 20x, 50x, 100x, etc.
                var multiplier = (step % 3 === 1) ? 2.5 : 2;
                tickFrequency = tickFrequency * multiplier;

                step++;
            }

            return tickFrequency;
        };

        var drawD3Chart = function (data) {
            // Hack to fix lack of data reloading
            document.getElementById('results-chart').innerHTML = '';

            data = data.sort(function (a, b) { return b.Sum - a.Sum; });

            var chartHeight = Math.min(data.length * 60, 600);
            var chartWidth = Math.min(600, document.getElementById('results-chart').offsetWidth);

            var longestTextWidth = d3.max(data, function (d) { return textWidth(d.Name); });

            var margin = { top: 30, right: 10, bottom: 10, left: longestTextWidth + 10 };
            var width = chartWidth - margin.left - margin.right;
            var height = chartHeight - margin.top - margin.bottom;

            var minX = d3.min(data, function (d) { return d.Sum; });
            var maxX = d3.max(data, function (d) { return d.Sum; });

            var x = d3.scale.linear()
                .range([0, width])
                .domain([Math.min(0, minX), Math.max(0, maxX)])
                .nice();

            var dataRange = x.domain()[1] - x.domain()[0];
            var tickFrequency = tickFrequencyForRange(dataRange);

            var y = d3.scale.ordinal()
                .rangeRoundBands([0, height], 0.1)
                .domain(data.map(function (d) { return d.Name; }));

            var xAxis = d3.svg.axis()
                .scale(x)
                .orient('top')
                .ticks(dataRange / tickFrequency); // Ticks is *how many* ticks we display, not the frequency...

            var yAxis = d3.svg.axis()
                .scale(y)
                .orient('left');

            var tooltip = d3.tip()
                .attr('class', 'd3-tip')
                .offset([-10, 0])
                .html(function (d) {
                    var voterNames = d.Voters.slice(0, 5).map(function (d) { return d.Name + ' (' + d.Value + ')'; });
                    var tooltipText = '<b>' + d.Name + '</b>: ' + d.Sum + ' votes<br /><br />' + voterNames.join('<br />');
                    // Clip for more than 5 voters
                    if (d.Voters.length > 5) {
                        tooltipText += '<br />+ ' + (d.Voters.length - 5) + ' others';
                    }

                    return tooltipText;
                });

            var chart = d3.select('#results-chart')
                .append('svg')
                    .attr('width', chartWidth)
                    .attr('height', chartHeight)
                .append('g')
                    .attr('transform', 'translate(' + margin.left + ',' + margin.top + ')');

            chart.append('g')
                .attr('class', 'x axis')
                .call(xAxis);

            chart.append('g')
                .attr('class', 'y axis')
                .call(yAxis)
                .append('line')
                    .attr('x1', x(0))
                    .attr('x2', x(0))
                    .attr('y2', height);

            chart.selectAll('.bar')
                .data(data)
                .enter()
                .append('rect')
                    .attr('class', function (d) { return d.Sum < 0 ? 'bar negative' : 'bar positive'; })
                    .attr('x', function (d) {
                        return x(Math.min(0, d.Sum));
                    })
                    .attr('y', function (d) {
                        return y(d.Name);
                    })
                    .attr('width', function (d) {
                        return Math.abs(x(-d.Sum) - x(0));
                    })
                    .attr('height', y.rangeBand())
                    .on('mouseover', tooltip.show)
                    .on('mouseout', tooltip.hide);

            chart.call(tooltip);
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
                var optionVote = { Name: d.VoterName, Value: d.VoteValue };
                groupedData[d.OptionName].Voters.push(optionVote);

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

            drawD3Chart(datapoints);
        }

        VoteService.refreshLastChecked(pollId);
        reloadData();
        setInterval(reloadData, 3000);
    }
})();
