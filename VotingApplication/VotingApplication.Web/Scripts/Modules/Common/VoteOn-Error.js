(function () {
    'use strict';

    angular
        .module('VoteOn-Error', ['VoteOn-Common', 'VoteOn-Account'])
            .config([
            '$routeProvider',
            function ($routeProvider) {
                $routeProvider
                    .when('/404', {
                        templateUrl: function () {
                            return '../Shared/NotFound';
                        }
                    })
                    .when('/GenericError', {
                        templateUrl: function () {
                            return '../Shared/GenericError';
                        }
                    })
                    .otherwise({
                        templateUrl: function () {
                            return '../Shared/GenericError';
                        }
                    });
            }]);
})();