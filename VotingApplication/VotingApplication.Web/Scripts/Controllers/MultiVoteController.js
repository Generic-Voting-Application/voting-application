/// <reference path="../Services/IdentityService.js" />
/// <reference path="../Services/PollService.js" />
/// <reference path="../Services/TokenService.js" />
/// <reference path="../Services/VoteService.js" />
(function () {
    angular
        .module('GVA.Voting')
        .controller('MultiVoteController', MultiVoteController);

    MultiVoteController.$inject = ['$scope', '$routeParams', 'IdentityService', 'PollService', 'TokenService', 'VoteService'];

    function MultiVoteController($scope, $routeParams, IdentityService, PollService, TokenService, VoteService) {

        var pollId = $routeParams.pollId;
        var token = null;

        $scope.submitVote = submiteVote;

        activate();


        function activate() {
            PollService.getPoll(pollId, getPollDataSuccessCallback);
        }

        function getPollDataSuccessCallback(pollData) {
            $scope.options = pollData.Options;

            TokenService.getToken(pollId, getTokenSuccessCallback);
        }

        function getTokenSuccessCallback(tokenData) {
            token = tokenData;

            VoteService.getTokenVotes(pollId, token, getTokenVotesSuccessCallback);
        }

        function getTokenVotesSuccessCallback(voteData) {
            voteData.forEach(function (dataItem) {

                for (var i = 0; i < $scope.options.length; i++) {
                    var option = $scope.options[i];

                    if (option.Id === dataItem.OptionId) {
                        option.voteValue = dataItem.VoteValue;
                        break;
                    }
                }
            });
        }

        function submiteVote(options) {
            if (!options) {
                return null;
            }

            if (!token) {
                // Probably invite only, tell the user
            }
            else if (!IdentityService.identity) {
                IdentityService.openLoginDialog($scope, openLoginDialogCallback);
            }
            else {

                var votes = options
                        .filter(function (option) { return option.voteValue; })
                        .map(function (option) {
                            return {
                                OptionId: option.Id,
                                VoteValue: option.voteValue,
                                VoterName: IdentityService.identity.name
                            };
                        });

                VoteService.submitVote(pollId, votes, token, submitVoteSuccessCallback);
            }
        }

        function openLoginDialogCallback() {
            $scope.submitVote($scope.options);
        }

        function submitVoteSuccessCallback() {
            window.location = $scope.$parent.resultsLink;
        }
    }

})();
