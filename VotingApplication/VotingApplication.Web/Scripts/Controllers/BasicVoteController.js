/// <reference path="../Services/IdentityService.js" />
/// <reference path="../Services/PollService.js" />
/// <reference path="../Services/TokenService.js" />
/// <reference path="../Services/VoteService.js" />
(function () {
    angular
        .module('GVA.Voting')
        .controller('BasicVoteController', BasicVoteController);

    BasicVoteController.$inject = ['$scope', '$routeParams', 'IdentityService', 'PollService', 'TokenService', 'VoteService'];

    function BasicVoteController($scope, $routeParams, IdentityService, PollService, TokenService, VoteService) {

        var pollId = $routeParams.pollId;
        var token = null;

        activate();


        $scope.submitVote = submitVote;

        function activate() {
            PollService.getPoll(pollId, getPollSuccessCallback);

            function getPollSuccessCallback(pollData) {
                $scope.options = pollData.Options;

                TokenService.getToken(pollId, getTokenSuccessCallback);
            }

            function getTokenSuccessCallback(tokenData) {
                token = tokenData;

                VoteService.getTokenVotes(pollId, token, getTokenVotesSuccessCallback);
            };

            function getTokenVotesSuccessCallback(voteData) {

                if (!voteData || voteData.length == 0) {
                    return;
                }

                var vote = voteData[0];

                angular.forEach($scope.options, function (option) {
                    if (option.Id === vote.OptionId) {
                        option.selected = true;
                    }
                });
            };

        };

        function submitVote(option) {
            if (!option) {
                return null;
            }

            if (!token) {
                // Probably invite only, tell the user
            }
            else if (!IdentityService.identity) {
                IdentityService.openLoginDialog($scope, function () {
                    $scope.submitVote(option);
                });
            }
            else {
                var votes = [{
                    OptionId: option.Id,
                    VoteValue: 1,
                    VoterName: IdentityService.identity.name
                }];

                VoteService.submitVote(pollId, votes, token, function () {
                    window.location = $scope.$parent.resultsLink;
                });
            }
        }

    };


})();
