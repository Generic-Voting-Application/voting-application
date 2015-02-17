(function () {
    var VotingApp = angular.module('VotingApp', ['ngRoute']);

    var configFunction = function ($routeProvider) {
        $routeProvider.
            when('/voting/:pollId', {
                templateUrl: 'routes/voting'
            })
            .when('/results', {
                templateUrl: 'routes/results'
            })
            .otherwise({
                redirectTo: '/voting'
            });
    }

    configFunction.$inject = ['$routeProvider'];

    VotingApp.config(configFunction);
})();
