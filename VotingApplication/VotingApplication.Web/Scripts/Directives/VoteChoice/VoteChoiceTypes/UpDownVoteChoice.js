(function () {
    'use strict';

    angular
        .module('VoteOn-Vote')
        .directive('upDownVoteChoice', upDownVoteChoice);

    function upDownVoteChoice() {
        return {
            restrict: 'E',
            scope: {
                choices: '='
            },
            templateUrl: '/Scripts/Directives/VoteChoice/VoteChoiceTypes/UpDownVoteChoice.html',
            controller: UpDownChoiceController
        };
    }

    UpDownChoiceController.$inject = ['$scope'];

    function UpDownChoiceController($scope) {

        $scope.downSelected = downSelected;
        $scope.neitherSelected = neutralSelected;
        $scope.upSelected = upSelected;

        function downSelected(choice) {
            choice.voteValue = -1;
        }

        function neutralSelected(choice) {
            choice.voteValue = 0;
        }

        function upSelected(choice) {
            choice.voteValue = 1;
        }
    }
})();