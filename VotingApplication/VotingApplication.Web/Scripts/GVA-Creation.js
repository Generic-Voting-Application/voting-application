(function () {
    angular
        .module('GVA.Creation', ['ngRoute', 'ngDialog', 'ngStorage', 'toggle-switch', 'GVA.Common', 'GVA.Voting'])
        .config(['$routeProvider',
        function ($routeProvider) {
            $routeProvider
                .when('/Manage/', {
                    templateUrl: '../Routes/Manage'
                })
                .when('/Manage/:manageId', {
                    templateUrl: '../Routes/Manage'
                })
                .when('/Manage/:manageId/Name', {
                    templateUrl: '../Routes/ManageName'
                })
                .when('/Manage/:manageId/Options', {
                    templateUrl: '../Routes/ManageOptions'
                })
                .when('/Manage/:manageId/Voters', {
                    templateUrl: '../Routes/ManageVoters'
                })
                .when('/Manage/:manageId/PollType', {
                    templateUrl: '../Routes/ManagePollType'
                })
                .when('/Manage/:manageId/Expiry', {
                    templateUrl: '../Routes/ManageExpiry'
                })
                .otherwise({
                    templateUrl: '../Routes/BasicCreate'
                });
        }]);
        // TODO: GVA.Voting should not be required, it should be GVA.Polls, but it's not been created yet.
})();
