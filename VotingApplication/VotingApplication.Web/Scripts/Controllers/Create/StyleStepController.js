

(function () {
    'use strict';

    angular
        .module('VoteOn-Create')
        .controller('StyleStepController', StyleStepController);

    StyleStepController.$inject = ['$scope'];

    function StyleStepController($scope) {

        $scope.changeMaxPerVote = changeMaxPerVote;
        $scope.changeMaxPoints = changeMaxPoints;
        $scope.initalisePointsConfig = initalisePointsConfig;

        function changeMaxPerVote(amount) {
            var newMaxPerVote = $scope.newPoll.MaxPerVote + amount;

            if (maxPerVoteIsValid(newMaxPerVote)) {
                $scope.newPoll.MaxPerVote = newMaxPerVote;
            }
        }

        function changeMaxPoints(amount) {
            var newMaxPoints = $scope.newPoll.MaxPoints + amount;

            if (maxPointsIsValid(newMaxPoints)) {
                $scope.newPoll.MaxPoints = newMaxPoints;
            }
        }

        function maxPointsIsValid(maxPoints) {
            return maxPoints > 0 && maxPoints >= $scope.newPoll.MaxPerVote;
        }

        function maxPerVoteIsValid(pointsPerVote) {
            return pointsPerVote > 0 && pointsPerVote <= $scope.newPoll.MaxPoints
        }

        function initalisePointsConfig() {
            if (!$scope.newPoll.MaxPerVote) {
                $scope.newPoll.MaxPerVote = 3;
            }

            if (!$scope.newPoll.MaxPoints) {
                $scope.newPoll.MaxPoints = 7;
            }
        }
    }
})();
