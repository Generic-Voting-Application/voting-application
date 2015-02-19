(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.controller('PointsVoteController', ['$scope', 'IdentityService', 'PollService', 'TokenService', function ($scope, IdentityService, PollService, TokenService) {

        var pollId = PollService.currentPollId();
        var token = null;

        $scope.options = [];
        $scope.totalPointsAvailable = 0;
        $scope.maxPointsPerOption = 0;

        $scope.vote = function (options) {
            if (!options) {
                return null;
            }

            if (!token) {
                // Probably invite only, tell the user
            } else if (!IdentityService.identityName) {
                IdentityService.openLoginDialog($scope, function () {
                    $scope.vote(options);
                });
            } else {

                votes = options
                    .filter(function (option) { return option.voteValue })
                    .map(function (option) {
                        return {
                            OptionId: option.Id,
                            VoteValue: option.voteValue,
                            VoterName: IdentityService.identityName
                        }
                    });

                PollService.submitVote(pollId, votes, token, function (data) {
                    console.log(data);
                });
            }
        }

        $scope.unallocatedPoints = function () {
            var unallocatedPoints = $scope.totalPointsAvailable;

            for (var i = 0; i < $scope.options.length; i++) {
                unallocatedPoints -= $scope.options[i].voteValue || 0;
            }

            return unallocatedPoints;
        }

        $scope.disabledAddPoints = function (pointValue) {
            return pointValue >= $scope.maxPointsPerOption || $scope.unallocatedPoints() === 0;
        }

        PollService.getPoll(pollId, function (data) {
            $scope.options = data.Options;
            $scope.options.forEach(function (d) {
                d.voteValue = 0;
            });
            $scope.totalPointsAvailable = data.MaxPoints;
            $scope.maxPointsPerOption = data.MaxPerVote;
        });

        TokenService.getToken(pollId, function (data) {
            token = data;
        });
    }]);
})();
