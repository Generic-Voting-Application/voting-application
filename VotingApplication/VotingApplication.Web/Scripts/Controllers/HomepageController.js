/// <reference path="../Services/AccountService.js" />
/// <reference path="../Services/PollService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Creation')
        .controller('HomepageController', HomepageController);

    HomepageController.$inject = ['$scope', 'AccountService'];

    function HomepageController($scope, AccountService) {

        $scope.isLoggedIn = false;

        activate();


        function activate() {
            AccountService.registerAccountObserver(function () {
                $scope.isLoggedIn = (AccountService.account !== null);
            });

            $scope.isLoggedIn = (AccountService.account !== null);
        }
    }

})();
