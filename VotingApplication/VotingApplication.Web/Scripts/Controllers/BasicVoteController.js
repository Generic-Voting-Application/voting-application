/// <reference path="../Services/IdentityService.js" />
/// <reference path="../Services/VoteService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Voting')
        .controller('BasicVoteController', BasicVoteController);

    BasicVoteController.$inject = ['$scope', '$routeParams', 'IdentityService', 'VoteService', 'ngDialog'];

    function BasicVoteController($scope, $routeParams, IdentityService, VoteService, ngDialog) {

        $scope.addOption = addOption;
        $scope.notifyOptionAdded = notifyOptionAdded;

        activate();

        function activate() {
            // Register our getVotes strategy with the parent controller
            $scope.setVoteCallback(getVotes);

            VoteService.getTokenVotes($scope.pollId, $scope.token, selectOption);
        }

        function selectOption(voteData) {

            if (!voteData || voteData.length === 0) {
                return;
            }

            var vote = voteData[0];

            angular.forEach($scope.poll.Options, function (option) {
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
                data: { pollId: $scope.pollId }
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
