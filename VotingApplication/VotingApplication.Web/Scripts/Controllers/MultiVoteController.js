(function () {
    'use strict';

    angular
        .module('GVA.Voting')
        .controller('MultiVoteController', MultiVoteController);

    MultiVoteController.$inject = ['$scope', '$routeParams', 'ngDialog'];

    function MultiVoteController($scope, $routeParams, ngDialog) {

        $scope.addChoice = addChoice;
        $scope.notifyChoiceAdded = notifyChoiceAdded;

        activate();

        function activate() {
            // Register our getVotes strategy with the parent controller
            $scope.setVoteCallback(getVotes);
        }

        function getVotes(choices) {
            return choices
                .filter(function (choice) { return choice.voteValue; })
                .map(function (choice) {
                    return {
                        ChoiceId: choice.Id,
                        VoteValue: 1
                    };
                });
        }

        function addChoice() {
            ngDialog.open({
                template: '/Routes/AddChoiceDialog',
                controller: 'AddVoterChoiceDialogController',
                scope: $scope,
                data: { pollId: $scope.pollId }
            });
        }

        function notifyChoiceAdded() {
            $scope.$emit('voterChoiceAddedEvent');
        }
    }
})();
