(function () {
    'use strict';

    angular
        .module('VoteOn-Account', ['ngMaterial', 'ngMessages', 'ngRoute'])
        .config(['$routeProvider',
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
            }])
                .config(function ($mdThemingProvider) {
                    $mdThemingProvider.theme('default')
                      .primaryPalette('teal')
                      .accentPalette('pink')
                      .warnPalette('red');

                    $mdThemingProvider.theme('default-dark')

                      .primaryPalette('blue')
                       .accentPalette('pink')
                       .warnPalette('red')
                      .dark();
                });
})();
