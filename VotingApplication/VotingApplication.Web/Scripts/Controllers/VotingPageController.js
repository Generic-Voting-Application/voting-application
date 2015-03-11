/// <reference path="../Services/IdentityService.js" />
(function () {
    angular
        .module('GVA.Voting')
        .controller('VotingPageController', VotingPageController);


    VotingPageController.$inject = ['$scope', '$location', 'IdentityService'];

    function VotingPageController($scope, $location, IdentityService) {

        // Turn "/#/voting/abc/123" into "/#/results/abc/123"
        var locationTokens = $location.url().split("/");
        locationTokens.splice(0, 2);

        $scope.resultsLink = '#/Results/' + locationTokens.join("/");
        $scope.identityName = IdentityService.identity ? IdentityService.identity.name : null;
        $scope.logoutIdentity = IdentityService.clearIdentityName;

        activate();


        function activate() {
            // Angular won't auto update this so we need to use the observer pattern
            IdentityService.registerIdentityObserver(function () {
                $scope.identityName = IdentityService.identity ? IdentityService.identity.name : null;;
            });
        }

    }
})();
