/// <reference path="../Services/IdentityService.js" />
/// <reference path="../Services/PollService.js" />
/// <reference path="../Services/TokenService.js" />
/// <reference path="../Services/VoteService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Voting')
        .controller('BasicVoteController', BasicVoteController);

    BasicVoteController.$inject = ['$scope', '$routeParams', 'IdentityService', 'PollService', 'TokenService', 'VoteService', 'ngDialog'];

    function BasicVoteController($scope, $routeParams, IdentityService, PollService, TokenService, VoteService, ngDialog) {

        var pollId = $routeParams.pollId;
        var token = null;


        $scope.options = {};
        $scope.optionAddingAllowed = false;

        $scope.addOption = addOption;
        $scope.notifyOptionAdded = notifyOptionAdded;

        // Register our getVotes strategy with the parent controller
        $scope.setVoteCallback(getVotes);

        activate();

        function activate() {
            $scope.$watch('poll', function () {
                if ($scope.poll) {
                    $scope.options = $scope.poll.Options;
                    $scope.optionAddingAllowed = $scope.poll.OptionAddingAllowed;
                }
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

        function addOption() {
            ngDialog.open({
                template: '/Routes/AddOptionDialog',
                controller: 'AddVoterOptionDialogController',
                scope: $scope,
                data: { pollId: pollId }
            });
        }

        function notifyOptionAdded() {
            $scope.$emit('voterOptionAddedEvent');
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
