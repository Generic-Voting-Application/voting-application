(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.controller('PointsVoteController', ['$scope', 'PollService', 'IdentityService', function ($scope, PollService, IdentityService) {

        $scope.options = [];
        $scope.totalPointsAvailable = 0;
        $scope.maxPointsPerOption = 0;

        $scope.vote = function (options) {
            if (IdentityService.identityName) {
                // Do the voting stuff here
                console.log(options);
            } else {
                IdentityService.openLoginDialog($scope);
            }
        }

        $scope.unallocatedPoints = function () {
            var unallocatedPoints = $scope.totalPointsAvailable;

            for (var i = 0; i < $scope.options.length; i++) {
                unallocatedPoints -= $scope.options[i].pollValue || 0;
            }

            return unallocatedPoints;
        }

        $scope.disabledAddPoints = function (pointValue) {
            return pointValue >= $scope.maxPointsPerOption || $scope.unallocatedPoints() === 0;
        }

        PollService.getPoll(PollService.currentPollId(), function (data) {
            $scope.options = data.Options;
            $scope.totalPointsAvailable = data.MaxPoints;
            $scope.maxPointsPerOption = data.MaxPerVote;
        });
    }]);
})();
