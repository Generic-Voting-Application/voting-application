(function () {
    'use strict';

    angular
        .module('VoteOn-Common', ['ngMaterial', 'ngMessages'])
        .config(function ($mdThemingProvider) {
            $mdThemingProvider.theme('default')
                .primaryPalette('blue')
                .accentPalette('pink');

            $mdThemingProvider.theme('default-dark')
                .primaryPalette('indigo')
                .accentPalette('blue')
                .dark();
            });
})();
