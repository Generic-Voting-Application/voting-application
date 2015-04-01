(function () {
    angular
        .module('GVA.Common', ['ngDialog', 'ngStorage'])
        .config(['$httpProvider', function ($httpProvider) {
            $httpProvider.interceptors.push('ErrorInterceptor');
        }]);
})();
