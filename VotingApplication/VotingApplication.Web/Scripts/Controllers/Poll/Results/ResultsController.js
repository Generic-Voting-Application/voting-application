(function () {
    'use strict';

    angular
        .module('VoteOn-Results')
        .controller('ResultsController', ResultsController);

    ResultsController.$inject = ['$scope', '$routeParams', 'ResultsService'];

    function ResultsController($scope, $routeParams, ResultsService) {

        $scope.pollId = $routeParams['pollId'];
        $scope.resultsBarChart = null;
        $scope.results = [];
        $scope.winners = [];

        activate();

        function activate() {
            ResultsService.getResults($scope.pollId)
            .then(function (data) {

                $scope.results = data.Results;
                $scope.winners = data.Winners;

                var chartData = ResultsService.createDataTable(data.Results);

                $scope.resultsBarChart = createBarChart(chartData, data.PollName);
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
