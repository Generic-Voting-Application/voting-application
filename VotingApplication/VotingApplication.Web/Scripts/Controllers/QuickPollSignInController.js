(function () {
    var VotingApp = angular.module('VotingApp');
    VotingApp.controller('QuickPollSignInController', function ($scope) {
        
        $scope.createPoll = function () {
            $scope.$parent.$parent.createPoll = true;

        }
    });
})();