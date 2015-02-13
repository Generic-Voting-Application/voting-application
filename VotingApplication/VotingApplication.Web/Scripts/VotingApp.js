var VotingApp = angular.module('VotingApp', ['ngRoute'])
.directive('votingStrategy', function () {
    return {
        templateUrl: votingStrategy
    }
});

var votingStrategy = function () {
    var pollId = currentPollId();

    // TODO: Actually find out the proper poll strategy with ajax
    if (pollId == '123')
        return 'routes/PointsVote';
    else
        return 'routes/BasicVote';
}

var currentPollId = function () {
    var urlParams = document.URL.split("#/")[1];
    var urlParamTokens = urlParams.split("/");

    if (urlParamTokens.length < 2)
        return;

    var pollId = urlParamTokens[1];

    return pollId;
}

VotingApp.controller('HomePageController', HomePageController);

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