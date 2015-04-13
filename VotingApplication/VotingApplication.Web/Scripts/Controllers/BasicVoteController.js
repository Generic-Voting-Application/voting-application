/// <reference path="../Services/IdentityService.js" />
/// <reference path="../Services/PollService.js" />
/// <reference path="../Services/TokenService.js" />
/// <reference path="../Services/VoteService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Voting')
        .controller('BasicVoteController', BasicVoteController);

    BasicVoteController.$inject = ['$scope', '$routeParams', 'IdentityService', 'PollService', 'TokenService', 'VoteService'];

    function BasicVoteController($scope, $routeParams, IdentityService, PollService, TokenService, VoteService) {

        var pollId = $routeParams.pollId;
        var token = null;

        // Register our getVotes strategy with the parent controller
        $scope.setVoteCallback(getVotes);

        activate();

        function activate() {
            $scope.$watch('poll', function () {
                $scope.options = $scope.poll ? $scope.poll.Options : [];
            });

            TokenService.getToken(pollId, getTokenSuccessCallback);
        }

        function getTokenSuccessCallback(tokenData) {
            token = tokenData;

            VoteService.getTokenVotes(pollId, token, getTokenVotesSuccessCallback);
        }

        function getTokenVotesSuccessCallback(voteData) {

            if (!voteData || voteData.length === 0) {
                return;
            }

            var vote = voteData[0];

            angular.forEach($scope.options, function (option) {
                if (option.Id === vote.OptionId) {
                    option.selected = true;
                }
            });
        }

        function getVotes(option) {
            return [{
                OptionId: option.Id,
                VoteValue: 1,
                VoterName: IdentityService.identity && $scope.poll && $scope.poll.NamedVoting ?
                           IdentityService.identity.name : null
            }];
        }
    }
})();
