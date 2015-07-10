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

        $scope.pointsChanged = pointsChanged;

        activate();

        function activate() {
            calculatePointsRemaining();
        }

        function pointsChanged() {
            calculatePointsRemaining();
        }

        function calculatePointsRemaining() {
            var totalAllocated = $scope.choices.reduce(function (previous, current) { return previous + current.VoteValue; }, 0);

            $scope.pointsRemaining = $scope.poll.MaxPoints - totalAllocated;
            $scope.percentagePointsRemaining = ($scope.pointsRemaining / $scope.poll.MaxPoints) * 100;
        }
    }
})();