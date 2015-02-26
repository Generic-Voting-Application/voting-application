(function () {
    angular.module('GVA.Creation').controller('CreatePageController', ['$scope', 'AccountService', function ($scope, AccountService) {

        $scope.openLoginDialog = function () {
            AccountService.openLoginDialog($scope);
        }

        $scope.openRegisterDialog = function () {
            AccountService.openRegisterDialog($scope);
        }

    }]);
})();
