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

        // Turn "/#/results/abc/123" into "/#/voting/abc/123"
        $scope.votingLink = RoutingService.getVotePageUrl(pollId, tokenId);

        $scope.winner = 'Lorem';
        //Whether or not we have an "s" on the end of "Winner"
        $scope.plural = '';

        $scope.chartData = [];

        $scope.voteCount = 0;

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

            $scope.chartData = datapoints;
        }

        VoteService.refreshLastChecked(pollId);
        reloadData();
        setInterval(reloadData, 3000);
    }
})();
