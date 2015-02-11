var VotingApp = angular.module('VotingApp', ['ngRoute']);

VotingApp.controller('HomePageController', HomePageController);

var configFunction = function ($routeProvider) {
    $routeProvider.
        when('/routeOne', {
            templateUrl: 'routes/one'
        })
        .when('/routeTwo', {
            templateUrl: 'routes/two'
        })
        .when('/routeThree', {
            templateUrl: 'routes/three'
        });
}
configFunction.$inject = ['$routeProvider'];

VotingApp.config(configFunction);