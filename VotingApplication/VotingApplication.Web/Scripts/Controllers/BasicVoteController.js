(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.controller('BasicVoteController', ['$scope', 'IdentityService', 'PollService', 'TokenService', function ($scope, IdentityService, PollService, TokenService) {

        var pollId = PollService.currentPollId();
        
        $scope.vote = function (option) {
            if (!option) {
                return null;
            }

            if (!IdentityService.identityName) {
                IdentityService.openLoginDialog($scope);
            } else if(!token) {
                // Probably invite only, tell the user
            } else {

                votes = [{
                    OptionId: option.Id,
                    VoteValue: 1,
                    VoterName: IdentityService.identityName
                }];

                PollService.submitVote(pollId, votes, token, function () {
                    console.log("voted!");
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
