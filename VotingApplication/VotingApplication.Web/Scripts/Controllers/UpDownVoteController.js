(function () {
    'use strict';

    angular
        .module('GVA.Voting')
        .controller('UpDownVoteController', UpDownVoteController);

    UpDownVoteController.$inject = ['$scope', '$routeParams', 'ngDialog'];

    function UpDownVoteController($scope, $routeParams, ngDialog) {

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
                        VoteValue: option.voteValue
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
