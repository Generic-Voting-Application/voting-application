(function () {
    angular
        .module('GVA.Common')
        .factory('ErrorInterceptor', ErrorInterceptor);

    ErrorInterceptor.$inject = ['$q', '$rootScope', 'ErrorService'];

    function ErrorInterceptor($q, $rootScope, ErrorService) {

        var interceptor = {
            'responseError': errorInterceptor,
            'response': responseInterceptor
        };

        return interceptor;

        function errorInterceptor(rejection) {

            if (rejection.status < 400) {
                return $q.reject(rejection);
            }

            $rootScope.error = rejection;
            

            var readableMessage = '';

            if (rejection.data && rejection.data.ModelState) {
                // Handle model state errors
                var firstError = rejection.data.ModelState[Object.keys(rejection.data.ModelState)[0]];
                readableMessage = firstError[0];
            } else if (rejection.data && rejection.data.error_description) {
                // Handle authentication errors
               readableMessage = rejection.data.error_description;
            } else if (rejection.statusText) {
                // Handle generic server messages
                readableMessage = rejection.statusText;
            } else {
                // Catch all for anything else
                readableMessage = rejection.data ? rejection.data.Message : "An error has occured";
            }

            readableMessage = ErrorService.createReadableString(readableMessage);

            $rootScope.error.readableMessage = readableMessage;

            return $q.reject(rejection);
        }

        function responseInterceptor(response) {

            $rootScope.error = null;

            return response;
        }
    }
})();
