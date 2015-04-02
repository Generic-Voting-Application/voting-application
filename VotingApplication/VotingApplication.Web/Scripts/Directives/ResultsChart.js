(function () {
    "use strict";

    angular
        .module('GVA.Voting')
        .directive('gvaResultsChart', resultsChart);

    function resultsChart() {

        function link($scope) {

            var canvas = document.createElement('canvas');

            $scope.$watch('data', function (newVal, oldVal) {
                if ((JSON.stringify(newVal) !== JSON.stringify(oldVal))) {
                    drawChart();
                }
            });

            function textWidth (text) {
                var context = canvas.getContext('2d');
                context.font = '16px Open Sans';
                var metrics = context.measureText(text);
                return metrics.width;
            }

            function tickFrequencyForRange(range) {
                var tickFrequency = 1;
                var step = 0;

                while (range / tickFrequency >= 10) {
                    // Multiply by: 2x, 5x, 10x, 20x, 50x, 100x, etc.
                    var multiplier = (step % 3 === 1) ? 2.5 : 2;
                    tickFrequency = tickFrequency * multiplier;

                    step++;
                }

                return tickFrequency;
            }

            function drawChart() {
                // Hack to fix lack of data reloading
                var chartElement = document.getElementById('inner-chart');
                chartElement.innerHTML = '';

                var data = $scope.data.sort(function (a, b) { return b.Sum - a.Sum; });

                var chartHeight = Math.min(data.length * 60, 600);
                var chartWidth = Math.min(600, chartElement.offsetWidth);

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

                var chart = d3.select(chartElement)
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
            }
        }

        return {
            restrict: 'E',
            scope: {
                data: '='
            },
            link: link,
            templateUrl: '/Scripts/Directives/ResultsChart.html'
        };
    }
})();