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

        activate();

        function activate() {
            $scope.choices.map(function (choice) {
                choice.selected = (choice.VoteValue === 1) ? true : false;
            });
        }

        function selectedChanged(choice) {
            choice.VoteValue = choice.selected ? 1 : 0;
        }
    }
})();