(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.controller('BasicVoteController', ['$scope', 'IdentityService', 'PollService', 'TokenService', function ($scope, IdentityService, PollService, TokenService) {

        var pollId = PollService.currentPollId();
        var token = null;

        $scope.vote = function (option) {
            if (!option) {
                return null;
            }

            if (!token) {
                // Probably invite only, tell the user
            } else if (!IdentityService.identityName) {
                IdentityService.openLoginDialog($scope, function () {
                    $scope.vote(option);
                });
            } else {
                votes = [{
                    OptionId: option.Id,
                    VoteValue: 1,
                    VoterName: IdentityService.identityName
                }];

                PollService.submitVote(pollId, votes, token, function () {
                    window.location = $scope.$parent.resultsLink;
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
