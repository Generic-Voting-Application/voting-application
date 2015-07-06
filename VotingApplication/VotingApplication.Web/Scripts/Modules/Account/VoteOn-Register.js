(function () {
    'use strict';

    angular
        .module('VoteOn-Register', ['VoteOn-Account'])
            .config([
            '$routeProvider',
            function ($routeProvider) {
                $routeProvider
                    .when('/RegistrationComplete/:emailAddress', {
                        templateUrl: function () {
                            return '../Register/RegistrationComplete';
                        }
                    })
                    .otherwise({
                        templateUrl: function () {
                            return '../Register/Register';
                        }
                    });
            }]);
})();