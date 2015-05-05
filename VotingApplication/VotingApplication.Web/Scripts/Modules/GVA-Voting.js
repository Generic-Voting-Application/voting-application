(function () {
    'use strict';

    angular
        .module('GVA.Voting', ['ngRoute', 'ngDialog', 'ngStorage', 'GVA.Common', 'GVA.Poll'])
        .config(['$routeProvider',
        function ($routeProvider) {
            $routeProvider.
                when('/Vote/:pollId/:tokenId?', {
                    templateUrl: function (params) {
                        return '../Routes/Vote/' + params['pollId'];
                    }
                })
                .when('/Results/:pollId/:tokenId?', {
                    templateUrl: function (params) {
                        return '../Routes/Results/' + params['pollId'];
                    }
                });
        }]);
})();
