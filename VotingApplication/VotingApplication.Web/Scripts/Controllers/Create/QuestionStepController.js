(function () {
    'use strict';

    angular
        .module('VoteOn-Create')
        .controller('QuestionStepController', QuestionStepController);

    QuestionStepController.$inject = ['$scope'];

    function QuestionStepController($scope) {

        $scope.pollHasChoices = pollHasChoices;

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
    }
})();
