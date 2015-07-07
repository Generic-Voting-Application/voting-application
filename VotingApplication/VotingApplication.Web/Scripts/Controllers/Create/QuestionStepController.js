(function () {
    'use strict';

    angular
        .module('VoteOn-Create')
        .controller('QuestionStepController', QuestionStepController);

    QuestionStepController.$inject = ['$scope'];

    function QuestionStepController($scope) {

        $scope.pollHasChoices = pollHasChoices;
        $scope.choiceChange = choiceChange;
        $scope.removeChoice = removeChoice;

        activate();

        function activate() {

            if($scope.newPoll) {
                $scope.newPoll.Choices = [
                    {
                        Name: ''
                    },
                    {
                        Name: ''
                    },
                    {
                        Name: ''
                    }
                ];
            }
        }

        function pollHasChoices() {
            return $scope.newPoll.Choices.some(function (choice) {
                return choice.Name;
            });
        }

        function choiceChange(index) {
            if (index === $scope.newPoll.Choices.length - 1) {
                $scope.newPoll.Choices.push({ Name: '' });
            }
        }

        function removeChoice(index) {
            $scope.newPoll.Choices.splice(index, 1);
        }
    }
})();
