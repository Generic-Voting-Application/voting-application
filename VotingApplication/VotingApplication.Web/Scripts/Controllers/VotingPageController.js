/// <reference path="../Services/IdentityService.js" />
(function () {
    angular
        .module('GVA.Voting')
        .controller('VotingPageController', VotingPageController);


    VotingPageController.$inject = ['$scope', '$routeParams', 'IdentityService'];

    function VotingPageController($scope, $routeParams, IdentityService) {

        // Turn "/#/voting/abc/123" into "/#/results/abc/123"
        var pollId = $routeParams['pollId'];
        var tokenId = $routeParams['tokenId'] || '';

        $scope.resultsLink = '#/Results/' + pollId + "/" + tokenId;
        $scope.identityName = IdentityService.identity ? IdentityService.identity.name : null;
        $scope.logoutIdentity = IdentityService.clearIdentityName;
        $scope.gvaExpiredCallback = redirectIfExpired;

        activate();

        function redirectIfExpired() {
            window.location.replace($scope.resultsLink);
        }

        function activate() {
            // Angular won't auto update this so we need to use the observer pattern
            IdentityService.registerIdentityObserver(function () {
                $scope.identityName = IdentityService.identity ? IdentityService.identity.name : null;
            });
        }

    }
})();
