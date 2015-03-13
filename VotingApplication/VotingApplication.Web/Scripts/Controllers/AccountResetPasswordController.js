/// <reference path="../Services/AccountService.js" />
/// <reference path="../Services/ErrorService.js" />
(function () {
    angular
        .module('GVA.Common')
        .controller('AccountResetPasswordController', AccountResetPasswordController);

    AccountResetPasswordController.$inject = ['$scope', '$routeParams', 'AccountService', 'ErrorService'];

    function AccountResetPasswordController($scope, $routeParams, AccountService, ErrorService) {

        var emailParameter = $routeParams.email;
        var codeParameter = $routeParams.code;

        $scope.email = emailParameter;

        $scope.resetPassword = resetPassword;

        function resetPassword(form) {

            AccountService.resetPassword(emailParameter, codeParameter, form.password, form.confirmpassword).success(function (data) {
                console.log('Reset success');
            }).error(function (data, status) {
                console.log('Reset failed');
            });
        }
    }

})();
