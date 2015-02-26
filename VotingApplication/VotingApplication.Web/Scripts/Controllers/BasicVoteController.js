(function () {
    angular.module('GVA.Voting').controller('BasicVoteController', ['$scope', 'IdentityService', 'PollService', 'TokenService', function ($scope, IdentityService, PollService, TokenService) {

        var pollId = PollService.currentPollId();
        var token = null;

        $scope.vote = function (option) {
            if (!option) {
                return null;
            }

            if (!token) {
                // Probably invite only, tell the user
            } else if (!IdentityService.identity) {
                IdentityService.openLoginDialog($scope, function () {
                    $scope.vote(option);
                });
            } else {
                votes = [{
                    OptionId: option.Id,
                    VoteValue: 1,
                    VoterName: IdentityService.identity.name
                }];

                PollService.submitVote(pollId, votes, token, function () {
                    window.location = $scope.$parent.resultsLink;
                });
            }
        }

        // Get Options
        PollService.getPoll(pollId, function (pollData) {
            $scope.options = pollData.Options;

            // Get Token
            TokenService.getToken(pollId, function (tokenData) {
                token = tokenData;

                // Get Previous Votes
                PollService.getTokenVotes(pollId, token, function (voteData) {
                    var vote = voteData[0];

                    angular.forEach($scope.options, function (option) {
                        if (option.Id === vote.OptionId) {
                            option.selected = true;
                        }
                    });
                });
            });

        });
    }]);

})();
