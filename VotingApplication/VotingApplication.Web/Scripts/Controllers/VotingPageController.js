/// <reference path="../Services/IdentityService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Voting')
        .controller('VotingPageController', VotingPageController);


    VotingPageController.$inject = ['$scope', '$routeParams', 'IdentityService', 'VoteService', 'TokenService', 'RoutingService', 'PollService'];

    function VotingPageController($scope, $routeParams, IdentityService, VoteService, TokenService, RoutingService, PollService) {

        // Turn "/#/voting/abc/123" into "/#/results/abc/123"
        var pollId = $routeParams['pollId'];
        var tokenId = $routeParams['tokenId'] || '';

        $scope.poll = null;
        $scope.resultsLink = RoutingService.getResultsPageUrl(pollId, tokenId);

        $scope.identityName = IdentityService.identity ? IdentityService.identity.name : null;
        $scope.logoutIdentity = IdentityService.clearIdentityName;
        $scope.gvaExpiredCallback = redirectIfExpired;
        $scope.submitVote = submitVote;

        var getVotes = function () { return []; };
        $scope.setVoteCallback = function (votesFunc) { getVotes = votesFunc; };

        activate();

        function redirectIfExpired() {
            window.location.replace($scope.resultsLink);
        }

        function activate() {
            // Angular won't auto update this so we need to use the observer pattern
            IdentityService.registerIdentityObserver(function () {
                $scope.identityName = IdentityService.identity ? IdentityService.identity.name : null;
            });

            TokenService.getToken(pollId, function (tokenData) {
                tokenId = tokenData;
            });

            PollService.getPoll(pollId, function (pollData) {
                $scope.poll = pollData;
            });
        }

        function submitVote(options) {
            if (!options) {
                return null;
            }

            if (!tokenId || tokenId.length === 0) {
                // TODO: Inform the user that they somehow don't have a token
                return;
            }

            if (!IdentityService.identity && $scope.poll && $scope.poll.NamedVoting) {
                return IdentityService.openLoginDialog($scope, function () {
                    submitVote(options);
                });
            }

            var votes = getVotes(options);
            
            VoteService.submitVote(pollId, votes, tokenId, function () {
                window.location = $scope.resultsLink;
            });
        }
    }
})();
