/// <reference path="../Services/IdentityService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Voting')
        .controller('MultiVoteController', MultiVoteController);

    MultiVoteController.$inject = ['$scope', '$routeParams', 'IdentityService', 'ngDialog'];

    function MultiVoteController($scope, $routeParams, IdentityService, ngDialog) {

        $scope.addOption = addOption;
        $scope.notifyOptionAdded = notifyOptionAdded;

        activate();

        function activate() {
            // Register our getVotes strategy with the parent controller
            $scope.setVoteCallback(getVotes);
        }

        function getVotes(options) {
            return options
                .filter(function (option) { return option.voteValue; })
                .map(function (option) {
                    return {
                        OptionId: option.Id,
                        VoteValue: 1,
                        VoterName: IdentityService.identity && $scope.poll && $scope.poll.NamedVoting ?
                                   IdentityService.identity.name : null
                    };
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
    }
})();
