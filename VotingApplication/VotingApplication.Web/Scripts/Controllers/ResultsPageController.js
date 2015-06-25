/// <reference path="../Services/IdentityService.js" />
/// <reference path="../Services/PollService.js" />
/// <reference path="../Services/VoteService.js" />
/// <reference path="../Services/RoutingService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Voting')
        .controller('ResultsPageController', ResultsPageController);

    ResultsPageController.$inject = ['$scope', '$routeParams', 'IdentityService', 'VoteService', 'RoutingService', 'PollService', 'TokenService', 'Errors'];

    function ResultsPageController($scope, $routeParams, IdentityService, VoteService, RoutingService, PollService, TokenService, Errors) {

        var pollId = $routeParams.pollId;
        var tokenId = $routeParams['tokenId'] || '';
        var reloadInterval = null;

        $scope.loaded = false;
        $scope.hasError = false;
        $scope.errorText = null;
        $scope.electionMode = false;

        $scope.votingLink = RoutingService.getVotePageUrl(pollId, tokenId);
        $scope.winner = 'Lorem';
        $scope.plural = '';

        $scope.chartData = [];

        $scope.hasVotes = false;
        $scope.hasExpired = false;
        $scope.gvaExpiredCallback = expire;

        activate();

        function activate() {

            var token = TokenService.retrieveToken(pollId);

            PollService.getPoll(pollId, token)
                .then(setPollDetails)
                .then(function () {
                    VoteService.refreshLastChecked(pollId);
                    reloadData(token);
                    reloadInterval = setInterval(function () { reloadData(token); }, 3000);

                    $scope.loaded = true;
                })
                .catch(handleError);
        }

        function setPollDetails(pollData) {
            if (pollData.ExpiryDateUtc) {
                $scope.hasExpired = moment.utc(pollData.ExpiryDateUtc).isBefore(moment.utc());
            }

            $scope.electionMode = pollData.ElectionMode;
        }

        function reloadData(token) {
            VoteService.getResults(pollId, token)
                .then(displayResults)
                .catch(handleError);
        }

        function displayResults(data) {
            if (!data) {
                return;
            }

            if (data.Winners) {
                $scope.hasVotes = data.Winners.length > 0;
            }

            if (data.Winners) {
                $scope.winner = data.Winners.join(', ');

                $scope.plural = (data.Winners.length > 1) ? 's (Draw)' : '';
            }

            if (data.Results) {
                var dataPoints = [];
                data.Results.forEach(function (result) {
                    dataPoints.push({ Name: result.ChoiceName, Sum: result.Sum, Voters: result.Voters });
                });

                $scope.chartData = dataPoints;
            }
        }

        function handleError(error) {

            if (reloadInterval) {
                clearInterval(reloadInterval);
            }

            if (error === Errors.IncorrectPollOrder) {
                RoutingService.navigateToVotePage(pollId, tokenId);
            }

            $scope.hasError = true;
            $scope.errorText = error.Text;
            $scope.loaded = true;
        }

        function expire() {
            $scope.hasExpired = true;
        }
    }
})();
