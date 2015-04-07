(function () {
    'use strict';

    angular
        .module('GVA.Common')
        .factory('ErrorInterceptor', ErrorInterceptor);

    ErrorInterceptor.$inject = ['$q', '$rootScope', 'ErrorService'];

    function ErrorInterceptor($q, $rootScope, ErrorService) {

        var interceptor = {
            'responseError': errorInterceptor,
            'request': requestInterceptor
        };

        return interceptor;

        function errorInterceptor(rejection) {

            if (rejection.status < 400) {
                return $q.reject(rejection);
            }

            $rootScope.error = rejection;
            

            var errorMessage = '';

            if (rejection.data && rejection.data.ModelState) {
                // Handle model state errors
                var firstError = rejection.data.ModelState[Object.keys(rejection.data.ModelState)[0]];
                errorMessage = firstError[0];
            } else if (rejection.data && rejection.data.error_description) {
                // Handle authentication errors
                errorMessage = rejection.data.error_description;
            } else if (rejection.statusText) {
                // Handle generic server messages
                errorMessage = rejection.statusText;
            } else {
                // Catch all for anything else
                errorMessage = rejection.data ? rejection.data.Message : 'An error has occured';
            }

            var readableMessage = ErrorService.createReadableString(errorMessage);

            $rootScope.error.readableMessage = readableMessage;

            return $q.reject(rejection);
        }

        function requestInterceptor(config) {

            $rootScope.error = null;

            return config;
        }
    }
})();
