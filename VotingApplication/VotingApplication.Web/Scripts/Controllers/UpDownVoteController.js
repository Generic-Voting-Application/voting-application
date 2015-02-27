(function () {
    angular.module('GVA.Voting').controller('UpDownVoteController', ['$scope', '$routeParams', 'IdentityService', 'PollService', 'TokenService',
            function ($scope, $routeParams, IdentityService, PollService, TokenService) {

                var pollId = $routeParams.pollId;
                var token = null;

                $scope.vote = function (options) {
                    if (!options) {
                        return null;
                    }

                    if (!token) {
                        // Probably invite only, tell the user
                    } else if (!IdentityService.identity) {
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
                                    VoterName: IdentityService.identity.name
                                }
                            });

                        PollService.submitVote(pollId, votes, token, function (data) {
                            window.location = $scope.$parent.resultsLink;
                        });
                    }
                }

                // Get Poll
                PollService.getPoll(pollId, function (pollData) {
                    $scope.options = pollData.Options;

                    // Get Token
                    TokenService.getToken(pollId, function (tokenData) {
                        token = tokenData;

                        // Get Previous Votes
                        PollService.getTokenVotes(pollId, token, function (voteData) {
                            voteData.forEach(function (dataItem) {
                                for (var i = 0; i < $scope.options.length; i++) {
                                    var option = $scope.options[i];
                                    if (option.Id === dataItem.OptionId) {
                                        option.voteValue = dataItem.VoteValue;
                                        break;
                                    }
                                };
                            });
                        });
                    });
                });
            }]);

})();
