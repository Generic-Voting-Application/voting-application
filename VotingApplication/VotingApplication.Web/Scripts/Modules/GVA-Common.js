(function () {
    'use strict';

    angular
        .module('GVA.Common', ['ngDialog', 'ngStorage', 'ngRoute'])
        .config(['$httpProvider', function ($httpProvider) {
            $httpProvider.interceptors.push('ErrorInterceptor');
        }]);
})();
