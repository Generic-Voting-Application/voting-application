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
        $scope.getUserPolls = getUserPolls;
        $scope.navigateToManagePage = navigateToManagePage;

        $scope.userPolls = {};

        activate();


        function activate() {
            AccountService.registerAccountObserver(function () {
                $scope.account = AccountService.account;
            });

            getUserPolls();
        }


        function createNewPoll(question) {
            PollService.createPoll(question, createPollSuccessCallback);
        }

        function createPollSuccessCallback(data) {
            navigateToManagePage(data.manageId);
        }

        function getUserPolls() {
            PollService.getUserPolls()
                .success(function (data) {
                    $scope.userPolls = data;
                });
        }

        function navigateToManagePage(manageId) {
            window.location.href = '/#/Manage/' + manageId;
        };
    }

})();
