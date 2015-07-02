(function () {
    'use strict';

    angular
        .module('VoteOn-Common', [])
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
