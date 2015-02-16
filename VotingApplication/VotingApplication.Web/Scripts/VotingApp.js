﻿(function () {
    var VotingApp = angular.module('VotingApp', ['ngRoute'])
    .directive('votingStrategy', ['pollData', function (pollData) {
        return {
            templateUrl: votingStrategy(pollData)
        }
    }]);

    var votingStrategy = function (pollData) {
        var pollId = pollData.currentPollId();

        // TODO: Actually find out the proper poll strategy with ajax
        if (pollId == '123')
            return 'routes/PointsVote';
        else
            return 'routes/BasicVote';
    }
    
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
