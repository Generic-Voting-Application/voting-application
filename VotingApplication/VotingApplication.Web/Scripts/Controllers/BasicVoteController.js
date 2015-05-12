(function () {
    'use strict';

    angular
        .module('GVA.Voting')
        .controller('BasicVoteController', BasicVoteController);

    BasicVoteController.$inject = ['$scope', '$routeParams', 'ngDialog'];

    function BasicVoteController($scope, $routeParams, ngDialog) {

        $scope.addOption = addOption;
        $scope.notifyOptionAdded = notifyOptionAdded;

        activate();

        function activate() {
            // Register our getVotes strategy with the parent controller
            $scope.setVoteCallback(getVotes);
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
                VoteValue: 1
            }];
        }
    }
})();
