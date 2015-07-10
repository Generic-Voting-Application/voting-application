(function () {
    'use strict';

    angular
        .module('VoteOn-Vote')
        .directive('pointsVoteChoice', pointsVoteChoice);

    function pointsVoteChoice() {
        return {
            restrict: 'E',
            scope: {
                choices: '=',
                poll: '='
            },
            templateUrl: '/Scripts/Directives/VoteChoice/VoteChoiceTypes/PointsVoteChoice.html',
            controller: PointsVoteController
        };
    }

    PointsVoteController.$inject = ['$scope'];

    function PointsVoteController($scope) {

        $scope.pointsRemaining = 0;
        $scope.percentagePointsRemaining = 0;

        $scope.decreasePoints = decreasePoints;
        $scope.decreasePointsDisabled = shouldDecreasePointsBeDisabled;
        $scope.increasePoints = increasePoints;
        $scope.increasePointsDisabled = shouldIncreasePointsBeDisabled;


        activate();

        function activate() {
            calculatePointsRemaining();
        }

        function calculatePointsRemaining() {
            var totalAllocated = $scope.choices.reduce(function (previous, current) { return previous + current.VoteValue; }, 0);

            $scope.pointsRemaining = $scope.poll.MaxPoints - totalAllocated;
            $scope.percentagePointsRemaining = ($scope.pointsRemaining / $scope.poll.MaxPoints) * 100;
        }

        function decreasePoints(choice) {
            if (choice.VoteValue > 0) {
                choice.VoteValue -= 1;
            }

            calculatePointsRemaining();
        }

        function shouldDecreasePointsBeDisabled(choice) {
            return choice.VoteValue <= 0;
        }

        function increasePoints(choice) {
            if (choice.VoteValue < $scope.poll.MaxPerVote) {
                choice.VoteValue += 1;
            }

            calculatePointsRemaining();
        }

        function shouldIncreasePointsBeDisabled(choice) {
            if (choice.VoteValue >= $scope.poll.MaxPerVote) {
                return true;
            }
            if ($scope.pointsRemaining === 0) {
                return true;
            }
            return false;
        }
    }
})();