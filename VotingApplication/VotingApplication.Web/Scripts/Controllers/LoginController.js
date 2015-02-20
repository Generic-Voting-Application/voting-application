(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.controller('LoginController', ['$scope', 'IdentityService', function ($scope, IdentityService) {
        
        $scope.loginIdentity = function (name) {
            IdentityService.setIdentityName(name);

            $scope.closeThisDialog();
            if ($scope.ngDialogData.callback) $scope.ngDialogData.callback();
        }

    }]);

})();
