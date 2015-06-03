(function () {
    'use strict';

    angular
        .module('GVA.Common')
        .filter('momentDisplayFilter', function () {
            return function (utcInput, format) {
                var utcDateTime = moment.utc(utcInput);
                return utcDateTime.local().format(format);
            };
        });
})();
