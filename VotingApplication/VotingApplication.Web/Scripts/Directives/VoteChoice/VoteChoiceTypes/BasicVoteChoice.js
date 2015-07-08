(function () {
    'use strict';

    angular
        .module('VoteOn-Vote')
        .directive('basicVoteChoice', basicVoteChoice);

    function basicVoteChoice() {
        return {
            restrict: 'E',
            scope: {
                choices: '='
            },
            templateUrl: '/Scripts/Directives/VoteChoice/VoteChoiceTypes/BasicVoteChoice.html',
            controller: BasicChoiceController
        };
    }

    BasicChoiceController.$inject = ['$scope'];

    function BasicChoiceController($scope) {
        $scope.choiceSelected = choiceSelected;

        function choiceSelected(choice) {

            $scope.choices.map(function (element) {
                if (element === choice && choice.VoteValue === 0) {
                    choice.VoteValue = 1;
                } else {
                    element.VoteValue = 0;
                }
            });

        }
    }
})();