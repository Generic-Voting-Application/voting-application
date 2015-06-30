(function () {
    'use strict';

    angular
        .module('VoteOn-Create', ['ngMaterial', 'ngMessages', 'mdDateTime'])
        .config(function ($mdThemingProvider) {
            $mdThemingProvider.theme('default')
              .primaryPalette('teal')
              .accentPalette('pink');

            $mdThemingProvider.theme('default-dark')

              .primaryPalette('teal')
               .accentPalette('pink')
              .dark();
        });
})();
