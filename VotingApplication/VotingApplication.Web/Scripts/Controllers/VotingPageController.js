(function () {
    var VotingApp = angular.module('VotingApp');
    VotingApp.controller('VotingPageController', ['$scope', 'AccountService', function ($scope, AccountService) {

        $scope.accountName = AccountService.accountName;

        // Angular won't auto update this so we need to use the observer pattern
        AccountService.registerAccountObserver(function () {
            $scope.accountName = AccountService.accountName;
        });

    }]);
})();
