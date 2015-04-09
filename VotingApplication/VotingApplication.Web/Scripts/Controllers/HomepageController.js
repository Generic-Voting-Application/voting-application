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
                setLoggedInValue();
            });

            setLoggedInValue();
        }

        function setLoggedInValue() {

            if (AccountService.account === undefined || AccountService.account === null) {
                $scope.isLoggedIn = false;
            }
            else {
                $scope.isLoggedIn = true;
            }
        }
    }
})();
