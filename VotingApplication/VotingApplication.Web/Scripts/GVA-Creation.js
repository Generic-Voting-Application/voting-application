(function () {
    angular.module('GVA.Creation', ['ngRoute', 'ngDialog', 'ngStorage', 'GVA.Common']).config(['$routeProvider', function ($routeProvider) {
        $routeProvider
           .when('/Manage/', {
                templateUrl: '../Routes/Manage'
            })
           .when('/Manage/:manageId', {
               templateUrl: '../Routes/Manage'
           })
           .otherwise({
               templateUrl: '../Routes/BasicCreate'
           })
    }]);
})();
