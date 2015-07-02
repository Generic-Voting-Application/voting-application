(function () {
    'use strict';

    angular
        .module('VoteOn-Login', ['VoteOn-Account'])
            .config([
            '$routeProvider',
            function ($routeProvider) {
                $routeProvider
                    .otherwise({
                        templateUrl: function () {
                            return '../Login/Login';
                        }
                    });
            }]);
})();