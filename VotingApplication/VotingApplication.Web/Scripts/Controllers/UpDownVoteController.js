(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.controller('UpDownVoteController', ['$scope', 'IdentityService', 'PollService', 'TokenService', function ($scope, IdentityService, PollService, TokenService) {

        var pollId = PollService.currentPollId();
        var token = null;

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

        PollService.getPoll(pollId, function (data) {
            $scope.options = data.Options;
        });

        TokenService.getToken(pollId, function (data) {
            token = data;
        });

    }]);

})();
