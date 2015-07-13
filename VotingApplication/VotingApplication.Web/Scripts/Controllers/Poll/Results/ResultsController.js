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

        var updateTimer = false;

        activate();

        function activate() {

            updateResults();

            updateTimer = $interval(updateResults, 5000);
            $scope.$on('$destroy', clearTimer);
        }

        function clearTimer() {
            $interval.cancel(updateTimer);
        }

        function updateResults() {
            ResultsService.getResults($scope.pollId)
           .then(function (data) {
               if (data) {
                   $scope.results = data.Results;
                   $scope.winners = data.Winners;

                   var resultsTrimSize = window.innerWidth / 30;
                   var chartData = ResultsService.createDataTable(data.Results, resultsTrimSize);

                   $scope.resultsBarChart = createBarChart(chartData, data.PollName);
                   if (!$scope.loaded) {
                       $scope.loaded = true;
                   }
               }
           })
           .catch(clearTimer);
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
