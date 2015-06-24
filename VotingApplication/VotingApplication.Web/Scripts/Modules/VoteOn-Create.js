(function () {
    'use strict';

    angular
        .module('VoteOn-Create', ['ngMaterial'])
        .config(function ($mdThemingProvider) {
            $mdThemingProvider.theme('default')
              .primaryPalette('blue');

            $mdThemingProvider.theme('default-dark')
              .primaryPalette('blue')
              .dark();
        });
})();
