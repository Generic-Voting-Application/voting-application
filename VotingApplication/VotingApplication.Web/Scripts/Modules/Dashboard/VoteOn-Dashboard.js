(function () {
    'use strict';

    angular
        .module('VoteOn-Dashboard', ['VoteOn-Common', 'VoteOn-Account'])
        .config([
                '$routeProvider',
                function ($routeProvider) {
                    $routeProvider
                        .when('/MyPolls', {
                            templateUrl: function () {
                                return '../Dashboard/MyPolls';
                            }
                        })
                        .when('/NotLoggedIn', {
                            templateUrl: function () {
                                return '../Dashboard/NotLoggedIn';
                            }
                        })
                        .otherwise({
                            templateUrl: function () {
                                return '../Dashboard/MyPolls';
                            }
                        });
                }]);
})();
