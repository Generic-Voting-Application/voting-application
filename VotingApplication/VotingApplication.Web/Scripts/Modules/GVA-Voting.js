(function () {
    angular
        .module('GVA.Voting', ['ngRoute', 'ngDialog', 'ngStorage', 'GVA.Common', 'GVA.Poll'])
        .config(['$routeProvider',
        function ($routeProvider) {
            $routeProvider.
                when('/Vote/:pollId/:tokenId?', {
                    templateUrl: '../Routes/Vote'
                })
                .when('/Results/:pollId/:tokenId?', {
                    templateUrl: '../Routes/Results'
                });
        }]);
})();
