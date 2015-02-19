(function () {
    var VotingApp = angular.module('VotingApp', ['ngRoute', 'ngDialog', 'ngStorage'])

    VotingApp.config(['$routeProvider', function ($routeProvider) {
        $routeProvider.
            when('/voting/:pollId', {
                templateUrl: 'routes/voting'
            })
            .when('/results/:pollId', {
                templateUrl: 'routes/results'
            })
            .otherwise({
                redirectTo: '/voting'
            });
    }]);
})();
