/// <reference path="../Services/IdentityService.js" />
/// <reference path="../Services/PollService.js" />
/// <reference path="../Services/VoteService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Voting')
        .controller('ResultsPageController', ResultsPageController);

    ResultsPageController.$inject = ['$scope', '$routeParams', 'IdentityService', 'VoteService', 'RoutingService', 'PollService'];

    function ResultsPageController($scope, $routeParams, IdentityService, VoteService, RoutingService, PollService) {

        var pollId = $routeParams.pollId;
        var tokenId = $routeParams['tokenId'] || '';
        var reloadInterval = null;

        $scope.votingLink = RoutingService.getVotePageUrl(pollId, tokenId);
        $scope.winner = 'Lorem';
        $scope.plural = '';

        $scope.chartData = [];

        $scope.voteCount = 0;
        $scope.hasExpired = false;
        $scope.gvaExpiredCallback = expire;

        activate();

        function expire() {
            $scope.hasExpired = true;
        }

        function reloadData() {
            VoteService.getResults(pollId, getResultsSuccessCallback, getResultsFailureCallback);
        }

        function getResultsFailureCallback(data, status) {
            if (status >= 400 && reloadInterval) {
                clearInterval(reloadInterval);
            }
        }

        function getResultsSuccessCallback(data) {

            if (!data) {
                return;
            }

            if (data.Votes) {
                $scope.voteCount = data.Votes.length;
            }
            
            if (data.Winners) {
                $scope.winner = data.Winners.map(function (d) {
                    return d.Name;
                }).join(', ');
                $scope.plural = (data.Winners.length > 1) ? 's (Draw)' : '';
            }

            if (data.Results) {
                var dataPoints = [];
                data.Results.forEach(function (result) {
                    dataPoints.push({ Name: result.Option.Name, Sum: result.Sum, Voters: result.Voters });
                });

                $scope.chartData = dataPoints;
            }

        }


        function getPollSuccessCallback(pollData) {
            if (pollData.ExpiryDate) {
                $scope.hasExpired = moment(pollData.ExpiryDate).isBefore(moment());
            }
        }

        function activate() {

            PollService.getPoll(pollId, getPollSuccessCallback);

            VoteService.refreshLastChecked(pollId);
            reloadData();
            reloadInterval = setInterval(reloadData, 3000);
        }

    }
})();
