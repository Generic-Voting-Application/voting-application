(function () {
    'use strict';

    angular
        .module('GVA.Common', ['ngDialog', 'ngStorage', 'ngRoute'])
        .config(['$httpProvider', function ($httpProvider) {
            $httpProvider.interceptors.push('ErrorInterceptor');
        }])

        .constant('Errors',
            {
                PollNotFound: { Id: 1, Text: 'Poll not found.' },
                NotAllowed: { Id: 2, Text: 'You are not allowed to access this.' }

            });
})();
