(function () {
    var VotingApp = angular.module('VotingApp');
    VotingApp.controller('CreatePageController', ['$scope', 'AccountService',  function ($scope, AccountService) {

        $scope.openLoginDialog = function () {
            AccountService.openLoginDialog($scope);
        }

    }]);
})();
