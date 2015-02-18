(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.controller('LoginController', ['$scope', 'AccountService', function ($scope, AccountService) {
        
        $scope.loginUser = function (name) {
            AccountService.setAccountName(name);

            $scope.closeThisDialog();
        }

    }]);

})();
