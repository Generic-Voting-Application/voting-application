(function () {
    'use strict';

    angular
        .module('VoteOn-Vote')
        .controller('VoteController', VoteController);

    VoteController.$inject = ['$scope', '$routeParams', 'TokenService', 'PollService', 'VoteService', 'RoutingService', 'IdentityService'];

    function VoteController($scope, $routeParams, TokenService, PollService, VoteService, RoutingService, IdentityService) {

        $scope.pollId = $routeParams['pollId'];
        $scope.poll = {
            Name: null,
            PollType: null,
            TokenGuid: null,
            ExpiryDateUtc: null,
            NamedVoting: null,
            Choices: []
        };

        $scope.voterIdentity = IdentityService.identity;

        $scope.castVote = castVote;

        activate();

        function activate() {
            getPollData();
        }

        function getPollData() {
            var token = TokenService.retrieveToken($scope.pollId);

            PollService.getPoll($scope.pollId, token)
                .then(function (data) {
                    TokenService.setToken($scope.pollId, data.TokenGuid);
                    return data;
                })
                .then(function (data) {
                    loadPollData(data);
                });
        }

        function loadPollData(data) {
            var poll = $scope.poll;
            poll.Name = data.Name;
            poll.PollType = data.PollType;
            poll.TokenGuid = data.TokenGuid;
            poll.ExpiryDateUtc = data.ExpiryDateUtc;
            poll.NamedVoting = data.NamedVoting;

            // Clear existing options
            poll.Choices.length = 0;
            poll.Choices = $scope.poll.Choices.concat(data.Choices);
        }

        function castVote() {

            if ($scope.poll.NamedVoting) {
                $scope.voteClicked = true;

                if ($scope.voterNameForm.$valid) {
                    submitVote($scope.voterIdentity.name);
                }
            } else {
                submitVote(null);
            }
        }

        function submitVote(voterName) {

            var votes = createVotes();

            var ballot = {
                VoterName: voterName,
                Votes: votes
            };

            VoteService.submitVote($scope.pollId, ballot, $scope.poll.TokenGuid)
                .then(function () {
                    RoutingService.navigateToResultsPage($scope.pollId);
                });
        }

        function createVotes() {
            return $scope.poll.Choices
                .filter(function (choice) {
                    return choice.VoteValue;
                })
                .map(function (choice) {
                    return {
                        ChoiceId: choice.Id,
                        VoteValue: choice.VoteValue
                    };
                });
        }
    }
})();
