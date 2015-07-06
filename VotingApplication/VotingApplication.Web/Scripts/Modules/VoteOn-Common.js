(function () {
    'use strict';

    angular
        .module('VoteOn-Common', [])
        .config(function ($mdThemingProvider) {
            $mdThemingProvider.theme('default')
                .primaryPalette('indigo')
                .accentPalette('indigo');

            $mdThemingProvider.theme('default-dark')
                .primaryPalette('indigo')
                .accentPalette('blue')
                .dark();
            });
})();
