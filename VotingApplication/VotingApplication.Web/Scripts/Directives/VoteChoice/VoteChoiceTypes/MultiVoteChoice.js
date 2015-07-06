(function () {
    'use strict';

    angular
        .module('VoteOn-Vote')
        .directive('multiVoteChoice', multiVoteChoice);

    function multiVoteChoice() {
        return {
            restrict: 'E',
            scope: {
                choices: '='
            },
            templateUrl: '/Scripts/Directives/VoteChoice/VoteChoiceTypes/MultiVoteChoice.html',
            controller: MultiVoteController
        };
    }

    MultiVoteController.$inject = ['$scope'];

    function MultiVoteController($scope) {

        $scope.selectedChanged = selectedChanged;

        function selectedChanged(choice) {
            choice.voteValue = choice.selected ? 1 : 0;
        }
    }
})();