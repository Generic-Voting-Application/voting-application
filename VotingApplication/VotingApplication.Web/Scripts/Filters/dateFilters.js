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

(function () {
    'use strict';

    angular
        .module('VoteOn-Common')
        .filter('dateFilter', function () {
            return function (utcInput, format) {
                var utcDateTime = moment.utc(utcInput);
                if (!utcDateTime.isValid()){
                    return 'Never';
                }
                return utcDateTime.local().format(format);
            };
        });
})();
