(function () {
    angular
        .module('GVA.Common')
        .factory('ErrorInterceptor', ErrorInterceptor);

    ErrorInterceptor.$inject = ['$q'];

    function ErrorInterceptor($q) {

        var interceptor = {
            'responseError': errorInterceptor
        };

        return interceptor;

        function errorInterceptor(rejection) {

            return $q.reject(rejection);
        }
    }
})();