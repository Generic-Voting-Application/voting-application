(function () {
    'use strict';

    angular
        .module('GVA.Help', ['ngRoute', 'GVA.Common'])
        .config(['$routeProvider',
        function ($routeProvider) {
            $routeProvider
                .when('/Voting/Basic/', {
                    templateUrl: '../Help/VotingBasic'
                })
                .when('/Voting/Points/', {
                    templateUrl: '../Help/VotingPoints'
                })
                .when('/Voting/UpDown/', {
                    templateUrl: '../Help/VotingUpDown'
                })
                .when('/Voting/Multi/', {
                    templateUrl: '../Help/VotingMulti'
                });
        }]);
})();
