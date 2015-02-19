(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.controller('PointsVoteController', ['$scope', 'IdentityService', 'PollService', 'TokenService', function ($scope, IdentityService, PollService, TokenService) {

        var pollId = PollService.currentPollId();
        var token = null;

        $scope.options = [];
        $scope.totalPointsAvailable = 0;
        $scope.maxPointsPerOption = 0;
        var pollId = PollService.currentPollId();
        var token;

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

                PollService.submitVote(pollId, votes, token, function () {
                    window.location = $scope.$parent.resultsLink;
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

        var getPreviousVotes = function () {
            PollService.getTokenVotes(pollId, token, function (data) {
                console.log(data);
                data.forEach(function (dataItem) {
                    for (var i = 0; i < $scope.options.length; i++) {
                        var option = $scope.options[i];
                        if (option.Id === dataItem.OptionId) {
                            option.voteValue = dataItem.VoteValue;
                            break;
                        }
                    };
                });
            });
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
            getPreviousVotes();
        });

    }]);
})();
