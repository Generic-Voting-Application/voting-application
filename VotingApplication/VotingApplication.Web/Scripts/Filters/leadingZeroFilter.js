(function () {
    'use strict';

    angular
        .module('VoteOn-Common')
        .filter('leadingZeroFilter', function () {
            return function (input) {
                return input <= 9 ? '0' + input : input;
            };
        });
})();
