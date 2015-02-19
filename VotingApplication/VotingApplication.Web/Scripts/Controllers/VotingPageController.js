(function () {
    var VotingApp = angular.module('VotingApp');
    VotingApp.controller('VotingPageController', ['$scope', 'IdentityService', function ($scope, IdentityService) {

        $scope.identityName = IdentityService.identityName;

        $scope.logoutIdentity = function () {
            IdentityService.clearIdentityName();
        }

        // Angular won't auto update this so we need to use the observer pattern
        IdentityService.registerIdentityObserver(function () {
            $scope.identityName = IdentityService.identityName;
        });

    }]);
})();
