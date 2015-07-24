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

        $scope.anyChoiceSelected = false;

        $scope.shouldShowSelectedCheck = shouldShowSelectedCheck;
        $scope.shouldOffsetchoice = shouldOffsetchoice;
        $scope.choiceSelected = choiceSelected;

        activate();

        function activate() {
            $scope.anyChoiceSelected = $scope.choices.some(function (elem) {
                return elem.VoteValue === 1;
            });
        }

        function choiceSelected(choice) {

            $scope.anyChoiceSelected = false;

            $scope.choices.map(function (element) {
                if (element === choice && choice.VoteValue === 0) {
                    choice.VoteValue = 1;
                    $scope.anyChoiceSelected = true;
                } else {
                    element.VoteValue = 0;
                }
            });
        }

        function shouldShowSelectedCheck(choice) {
            return !$scope.anyChoiceSelected || choice.VoteValue === 1;
        }


        function shouldOffsetchoice(choice) {
            return $scope.anyChoiceSelected && choice.VoteValue === 0;
        }
    }
})();