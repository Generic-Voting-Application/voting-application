(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.controller('IdentityLoginController', ['$scope', 'IdentityService', function ($scope, IdentityService) {
        
        $scope.loginIdentity = function (form) {
            IdentityService.setIdentityName(form.name);

            $scope.closeThisDialog();
            if ($scope.ngDialogData.callback) $scope.ngDialogData.callback();
        }

    }]);

})();
