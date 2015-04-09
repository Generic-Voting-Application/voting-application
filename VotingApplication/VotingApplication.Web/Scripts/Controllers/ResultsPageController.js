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

        function reloadData (){
            VoteService.getResults(pollId, getResultsSuccessCallback, getResultsFailureCallback);
        }

        function getResultsFailureCallback(data, status) {
            if (status >= 400 && reloadInterval) {
                clearInterval(reloadInterval);
            }
        }

        function getResultsSuccessCallback(data) {

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

            $scope.chartData = datapoints;
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
