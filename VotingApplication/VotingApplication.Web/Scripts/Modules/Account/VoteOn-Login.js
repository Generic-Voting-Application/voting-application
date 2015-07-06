(function () {
    'use strict';

    angular
        .module('VoteOn-Login', ['VoteOn-Account'])
            .config([
            '$routeProvider',
            function ($routeProvider) {
                $routeProvider
                    .when('/ForgottenPassword', {
                        templateUrl: function () {
                            return '../Login/ForgottenPassword';
                        }
                    })
                    .otherwise({
                        templateUrl: function () {
                            return '../Login/Login';
                        }
                    });
            }]);
})();