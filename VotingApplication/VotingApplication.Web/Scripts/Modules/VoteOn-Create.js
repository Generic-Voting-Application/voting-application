(function () {
    'use strict';

    angular
        .module('VoteOn-Create', ['ngMaterial', 'ngMessages', 'mdThemeColors'])
        .config(function ($mdThemingProvider) {
            $mdThemingProvider.theme('default')
              .primaryPalette('blue')
              .accentPalette('pink')
              .warnPalette('red');

            $mdThemingProvider.theme('default-dark')
              .primaryPalette('blue')
               .accentPalette('pink')
               .warnPalette('red')
              .dark();
        });
})();
