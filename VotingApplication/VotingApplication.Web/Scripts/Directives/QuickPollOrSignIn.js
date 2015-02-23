(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.directive('quickPollOrSignIn', function () {

        return {
            templateUrl: 'routes/create'
        }
    });
})();
