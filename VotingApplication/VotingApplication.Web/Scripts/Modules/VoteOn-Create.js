(function () {
    'use strict';

    angular
        .module('VoteOn-Create', ['ngMaterial'])
        .config(function ($mdThemingProvider) {
            $mdThemingProvider.theme('default')
              .primaryPalette('blue')
              .accentPalette('pink');

            $mdThemingProvider.theme('default-dark')
              .primaryPalette('blue')
              .dark();
        });
})();
