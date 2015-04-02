(function () {
    angular
        .module('GVA.Common')
        .factory('ErrorInterceptor', ErrorInterceptor);

    ErrorInterceptor.$inject = ['$q', '$rootScope'];

    function ErrorInterceptor($q, $rootScope) {

        var interceptor = {
            'responseError': errorInterceptor,
            'response': responseInterceptor
        };

        return interceptor;

        function errorInterceptor(rejection) {

            $rootScope.error = rejection;
            $rootScope.error.readableMessage = '';

            if (rejection.data && rejection.data.ModelState) {
                // Handle model state errors
                var firstError = rejection.data.ModelState[Object.keys(rejection.data.ModelState)[0]];
                $rootScope.error.readableMessage = firstError[0];
            } else if (rejection.data && rejection.data.error_description) {
                // Handle authentication errors
                $rootScope.error.readableMessage = rejection.data.error_description;
            } else if (rejection.statusText) {
                // Handle generic server messages
                $rootScope.error.readableMessage = rejection.statusText;
            } else {
                // Catch all for anything else
                $rootScope.error.readableMessage = rejection.data ? rejection.data.Message : "An error has occured";
            }

            return $q.reject(rejection);
        }

        function responseInterceptor(response) {

            $rootScope.error = null;

            return response;
        }
    }
})();
