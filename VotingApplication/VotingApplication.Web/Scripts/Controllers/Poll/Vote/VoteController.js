(function () {
    'use strict';

    angular
        .module('VoteOn-Vote')
        .controller('VoteController', VoteController);

    VoteController.$inject = ['$scope', '$routeParams', 'TokenService', 'PollService', 'VoteService', 'RoutingService'];

    function VoteController($scope, $routeParams, TokenService, PollService, VoteService, RoutingService) {

        $scope.loaded = false;

        $scope.pollId = $routeParams['pollId'];
        $scope.poll = {
            Name: null,
            PollType: null,
            TokenGuid: null,
            ExpiryDateUtc: null,
            NamedVoting: null,
            Choices: []
        };

        $scope.VoterName = '';

        $scope.castVote = castVote;
        $scope.pollExpired = pollExpired;

        activate();

        function activate() {
            getPollData();
        }

        function getPollData() {
            var token = TokenService.retrieveToken($scope.pollId);

            PollService.getPoll($scope.pollId, token)
                .then(function (data) {

                    TokenService.setToken($scope.pollId, data.TokenGuid)
                        .then(function () {
                            loadPollData(data);
                        });
                });
        }

        function loadPollData(data) {
            var poll = $scope.poll;
            poll.Name = data.Name;
            poll.PollType = data.PollType;
            poll.TokenGuid = data.TokenGuid;
            poll.ExpiryDateUtc = data.ExpiryDateUtc;
            poll.NamedVoting = data.NamedVoting;

            poll.MaxPoints = data.MaxPoints;
            poll.MaxPerVote = data.MaxPerVote;

            // Clear existing options
            poll.Choices.length = 0;
            poll.Choices = $scope.poll.Choices.concat(data.Choices);

            $scope.loaded = true;
        }

        function castVote() {

            if ($scope.poll.NamedVoting) {
                $scope.voteClicked = true;

                if ($scope.voterNameForm.$valid) {
                    var voterName = $scope.VoterName;

                    submitVote(voterName);
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

        function pollExpired() {
            RoutingService.redirectToResultsPage($scope.pollId);
        }
    }
})();
