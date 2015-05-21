(function () {
    'use strict';

    angular
        .module('GVA.Voting')
        .controller('BasicVoteController', BasicVoteController);

    BasicVoteController.$inject = ['$scope', '$routeParams', 'ngDialog'];

    function BasicVoteController($scope, $routeParams, ngDialog) {

        $scope.addChoice = addChoice;
        $scope.notifyChoiceAdded = notifyChoiceAdded;

        activate();

        function activate() {
            // Register our getVotes strategy with the parent controller
            $scope.setVoteCallback(getVotes);
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

        function getVotes(choice) {
            return [{
                ChoiceId: choice.Id,
                VoteValue: 1
            }];
        }
    }
})();
