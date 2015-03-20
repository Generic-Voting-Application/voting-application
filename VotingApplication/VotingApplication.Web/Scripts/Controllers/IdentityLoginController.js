/// <reference path="../Services/IdentityService.js" />
(function () {
    angular
        .module('GVA.Voting')
        .controller('IdentityLoginController', IdentityLoginController);

    IdentityLoginController.$inject = ['$scope', 'IdentityService'];

    function IdentityLoginController($scope, IdentityService) {

        $scope.loginIdentity = function (form) {
            IdentityService.setIdentityName(form.name);

            $scope.closeThisDialog();

            if ($scope.ngDialogData.callback) {
                $scope.ngDialogData.callback();
            }
        }
    }

})();
