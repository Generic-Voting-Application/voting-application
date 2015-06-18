/// <reference path="../Services/IdentityService.js" />
/// <reference path="../Services/PollService.js" />
/// <reference path="../Services/VoteService.js" />
/// <reference path="../Services/RoutingService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Voting')
        .controller('ResultsPageController', ResultsPageController);

    ResultsPageController.$inject = ['$scope', '$routeParams', 'IdentityService', 'VoteService', 'RoutingService', 'PollService', 'TokenService'];

    function ResultsPageController($scope, $routeParams, IdentityService, VoteService, RoutingService, PollService, TokenService) {

        var pollId = $routeParams.pollId;
        var tokenId = $routeParams['tokenId'] || '';
        var reloadInterval = null;

        $scope.loaded = false;
        $scope.hasError = false;
        $scope.errorText = null;

        $scope.votingLink = RoutingService.getVotePageUrl(pollId, tokenId);
        $scope.winner = 'Lorem';
        $scope.plural = '';

        $scope.chartData = [];

        $scope.voteCount = 0;
        $scope.hasExpired = false;
        $scope.gvaExpiredCallback = expire;

        activate();

        function activate() {

            var token = TokenService.retrieveToken(pollId);

            PollService.getPoll(pollId, token)
                .then(setExpiryDate)
                .then(function () {
                    VoteService.refreshLastChecked(pollId);
                    reloadData();
                    reloadInterval = setInterval(reloadData, 3000);

                    $scope.loaded = true;
                })
                .catch(function (error) {
                    $scope.hasError = true;
                    $scope.errorText = error.Text;
                    $scope.loaded = true;
                });
        }

        function setExpiryDate(pollData) {
            if (pollData.ExpiryDateUtc) {
                $scope.hasExpired = moment.utc(pollData.ExpiryDateUtc).isBefore(moment.utc());
            }
        }

        function reloadData() {
            VoteService.getResults(pollId)
                .then(displayResults)
                .catch(handleGetResultsError);
        }

        function displayResults(data) {
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
                    dataPoints.push({ Name: result.Choice.Name, Sum: result.Sum, Voters: result.Voters });
                });

                $scope.chartData = dataPoints;
            }
        }

        function handleGetResultsError() {
            if (reloadInterval) {
                clearInterval(reloadInterval);
            }
        }

        function expire() {
            $scope.hasExpired = true;
        }
    }
})();
