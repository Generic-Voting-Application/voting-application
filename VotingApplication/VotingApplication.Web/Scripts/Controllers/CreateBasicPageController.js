(function () {
    angular
        .module('GVA.Creation')
        .controller('CreateBasicPageController', CreateBasicPageController);

    CreateBasicPageController.$inject = ['$scope', 'AccountService'];

    function CreateBasicPageController($scope, AccountService) {

        $scope.openLoginDialog = function () {
            AccountService.openLoginDialog($scope);
        }

        $scope.openRegisterDialog = function () {
            AccountService.openRegisterDialog($scope);
        }

    };
})();
