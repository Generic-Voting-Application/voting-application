(function () {
    angular
        .module('GVA.Creation', ['ngRoute', 'ngDialog', 'ngStorage', 'toggle-switch', 'GVA.Common', 'GVA.Poll'])
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
                .when('/Manage/:manageId/Invitees', {
                    templateUrl: '../Routes/ManageInvitees'
                })
                .when('/Manage/:manageId/InvitationStyle', {
                    templateUrl: '../Routes/ManageInvitationStyle'
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
                .when('/Account/ResetPassword', {
                    templateUrl: '../Routes/AccountResetPassword'
                })
                .otherwise({
                    templateUrl: '../Routes/HomePage'
                });
        }]);
})();
