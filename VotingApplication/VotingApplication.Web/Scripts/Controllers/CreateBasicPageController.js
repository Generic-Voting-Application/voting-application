(function () {
    var VotingApp = angular.module('VotingApp');
    VotingApp.controller('CreateBasicPageController', ['$scope', 'AccountService', function ($scope, AccountService) {

        $scope.openLoginDialog = function () {
            AccountService.openLoginDialog($scope);
        }

        $scope.openRegisterDialog = function () {
            AccountService.openRegisterDialog($scope);
        }

    }]);
})();
