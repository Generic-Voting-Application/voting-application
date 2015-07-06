(function () {
    'use strict';

    angular
        .module('VoteOn-Vote')
        .controller('VoteController', VoteController);

    VoteController.$inject = ['$scope', '$routeParams', 'TokenService', 'PollService'];

    function VoteController($scope, $routeParams, TokenService, PollService) {

        $scope.pollId = $routeParams['pollId'];
        $scope.poll = {
            Name: null,
            PollType: null,
            Choices: []
        };

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

            // Clear existing options
            poll.Choices.length = 0;
            poll.Choices = $scope.poll.Choices.concat(data.Choices);
        }
    }
})();
