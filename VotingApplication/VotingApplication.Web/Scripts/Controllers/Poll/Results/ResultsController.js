(function () {
    'use strict';

    angular
        .module('VoteOn-Results')
        .controller('ResultsController', ResultsController);

    ResultsController.$inject = ['$scope', '$routeParams', '$interval', 'ResultsService'];

    function ResultsController($scope, $routeParams, $interval, ResultsService) {

        $scope.pollId = $routeParams['pollId'];
        $scope.resultsBarChart = null;
        $scope.results = [];
        $scope.winners = [];
        $scope.loaded = false;

        var updateTimer = null;

        activate();

        function activate() {

            updateResults().then(function () {
                $scope.loaded = true;
            });

            updateTimer = $interval(updateResults, 5000);
            $scope.$on('$destroy', clearTimer);
        }

        function clearTimer() {
            $interval.cancel(updateTimer);
        }

        function updateResults() {
            return ResultsService.getResults($scope.pollId)
           .then(function (data) {
               if (data) {
                   $scope.results = data.Results;
                   $scope.winners = filterDuplicates(data.Winners);

                   var resultsTrimSize = window.innerWidth / 30;
                   var chartData = ResultsService.createDataTable(data.Results, resultsTrimSize);

                   $scope.resultsBarChart = createBarChart(chartData, data.PollName);
               }
           })
           .catch(clearTimer);
        }

        function filterDuplicates(array) {
            var hash = {};
            return array.filter(function (item) {
                return hash.hasOwnProperty(item) ? false : (hash[item] = true);
            });
        }

        function createBarChart(chartData, pollName) {
            var barChart = {};
            barChart.type = 'google.charts.Bar';
            barChart.data = chartData;
            barChart.options = {
                height: 400,
                chart: {
                    title: 'Poll Results',
                    subtitle: pollName,
                },
                bars: 'horizontal',
                legend: {
                    position: 'none'
                }
            };

            return barChart;
        }
    }
})();
