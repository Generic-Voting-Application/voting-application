(function () {
    var VotingApp = angular.module('VotingApp', ['ngRoute', 'ngDialog', 'ngStorage'])

    VotingApp.config(['$routeProvider', function ($routeProvider) {
        $routeProvider.
            when('/Vote/:pollId', {
                templateUrl: '../Routes/Vote'
            })
            .when('/Results/:pollId', {
                templateUrl: '../Routes/Results'
            })
    }]);
})();
