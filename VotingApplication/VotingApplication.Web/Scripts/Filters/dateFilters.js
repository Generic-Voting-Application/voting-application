(function () {
    'use strict';

    angular
        .module('GVA.Common')
        .filter('momentFilter', function () {
            return function (input, format) {
                return moment(input).format(format);
            };
        });
})();
