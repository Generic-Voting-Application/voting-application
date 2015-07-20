(function () {
    'use strict';

    angular
        .module('VoteOn-Results')
        .controller('ResultsController', ResultsController);

    ResultsController.$inject = ['$scope', '$routeParams', '$interval', 'TokenService', 'ResultsService', 'ErrorService', 'SignalService'];

    function ResultsController($scope, $routeParams, $interval, TokenService, ResultsService, ErrorService, SignalService) {

        $scope.pollId = $routeParams['pollId'];
        $scope.resultsBarChart = null;
        $scope.results = [];
        $scope.winners = [];
        $scope.loaded = false;

        $scope.token = null;

        var updateTimer = null;

        activate();

        function activate() {

            $scope.token = TokenService.retrieveToken($scope.pollId);

            updateResults()
                .then(function () {
                    $scope.loaded = true;
                });

            updateTimer = $interval(updateResults, 5000);
            $scope.$on('$destroy', clearTimer);

            SignalService.registerObserver($scope.pollId, updateResults);
        }

        function updateResults() {
            return ResultsService.getResults($scope.pollId, $scope.token)
                .then(function (data) {

                    if (data) {
                        $scope.results = data.Results;

                        if (data.Winners) {
                            $scope.winners = filterDuplicates(data.Winners);
                        }

                        if (data.Results) {

                            var resultsTrimSize = window.innerWidth / 30;
                            var chartData = ResultsService.createDataTable(data.Results, resultsTrimSize);

                            $scope.resultsBarChart = createBarChart(chartData, data.PollName);
                        }
                    }

                })
                .catch(function (response) {
                    clearTimer();

                    ErrorService.handleResultsError(response);
                });
        }

        function clearTimer() {
            $interval.cancel(updateTimer);
        }

        function filterDuplicates(array) {
            var hashSet = {};
            return array.filter(function (item) {
                return Object.prototype.hasOwnProperty.call(hashSet, item) ? false : (hashSet[item] = true);
            });
        }

        function createBarChart(chartData, pollName) {
            var barChart = {};
            barChart.type = 'google.charts.Bar';
            barChart.data = chartData;
            barChart.options = {
                height: Math.max(400, chartData.length * 30),
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
