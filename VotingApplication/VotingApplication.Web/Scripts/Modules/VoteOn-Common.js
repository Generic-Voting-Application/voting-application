(function () {
    'use strict';

    angular
        .module('VoteOn-Common', ['ngMaterial', 'ngMessages', 'ngStorage', 'ngRoute'])
        .config(['$mdThemingProvider', function ($mdThemingProvider) {
            $mdThemingProvider.theme('default')
                .primaryPalette('blue')
                .accentPalette('pink');
        }]);
})();
