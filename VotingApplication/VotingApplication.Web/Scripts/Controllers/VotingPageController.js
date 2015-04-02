/// <reference path="../Services/IdentityService.js" />
(function () {
    angular
        .module('GVA.Voting')
        .controller('VotingPageController', VotingPageController);


    VotingPageController.$inject = ['$scope', '$routeParams', 'IdentityService', 'VoteService', 'TokenService', 'RoutingService'];

    function VotingPageController($scope, $routeParams, IdentityService, VoteService, TokenService, RoutingService) {

        // Turn "/#/voting/abc/123" into "/#/results/abc/123"
        var pollId = $routeParams['pollId'];
        var tokenId = $routeParams['tokenId'] || '';

        $scope.resultsLink = RoutingService.getResultsPageUrl(pollId, tokenId);
        $scope.identityName = IdentityService.identity ? IdentityService.identity.name : null;
        $scope.logoutIdentity = IdentityService.clearIdentityName;
        $scope.gvaExpiredCallback = redirectIfExpired;
        $scope.submitVotes = submitVotes;

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
        }

        function submitVotes(options) {
            if (!options) {
                return null;
            }

            if (!tokenId || tokenId.length === 0) {
                // TODO: Inform the user that they somehow don't have a token
                return;
            }

            if (!IdentityService.identity) {
                return IdentityService.openLoginDialog($scope, function () {
                    submitVotes(options);
                });
            }

            var votes = $scope.getVotes(options);

            VoteService.submitVote(pollId, votes, tokenId, function () {
                window.location = $scope.$parent.resultsLink;
            });
        }
    }
})();
