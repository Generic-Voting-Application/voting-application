/// <reference path="../Services/AccountService.js" />
/// <reference path="../Services/PollService.js" />
(function () {
    angular
        .module('GVA.Creation')
        .controller('DashboardController', DashboardController);

    DashboardController.$inject = ['$scope', 'AccountService', 'PollService'];

    function DashboardController($scope, AccountService, PollService) {

        $scope.account = AccountService.account;
        $scope.createPoll = createNewPoll;

        activate();


        function activate() {
            AccountService.registerAccountObserver(function () {
                $scope.account = AccountService.account;
            });
        }


        function createNewPoll(question) {
            PollService.createPoll(question, createPollSuccessCallback);
        }

        function createPollSuccessCallback(data) {
            window.location.href = '/#/Manage/' + data.ManageId;
        }
    }

})();
