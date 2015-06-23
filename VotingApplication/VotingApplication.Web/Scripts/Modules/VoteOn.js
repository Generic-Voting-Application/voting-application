(function () {
    'use strict';

    angular
        .module('VoteOn', ['ngMaterial'])
        .config(function ($mdThemingProvider) {
            $mdThemingProvider.theme('default')
              .primaryPalette('blue');
        });
})();
