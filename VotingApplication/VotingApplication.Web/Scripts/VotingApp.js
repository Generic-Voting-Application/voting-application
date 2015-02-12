var VotingApp = angular.module('VotingApp', ['ngRoute']);

VotingApp.controller('HomePageController', HomePageController);

var configFunction = function ($routeProvider) {
    $routeProvider.
        when('/voting', {
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