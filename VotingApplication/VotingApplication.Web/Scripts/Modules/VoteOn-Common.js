(function () {
    'use strict';

    angular
        .module('VoteOn-Common', [])
        .config(function ($mdThemingProvider) {
            $mdThemingProvider.theme('default')
                .primaryPalette('blue')
                .accentPalette('deep-orange');

            $mdThemingProvider.theme('default-dark')
                .primaryPalette('indigo')
                .accentPalette('blue')
                .dark();
            });
})();
