/// <reference path="../Services/AccountService.js" />
/// <reference path="../Services/PollService.js" />
(function () {
    angular
        .module('GVA.Creation')
        .controller('CreateBasicPageController', CreateBasicPageController);

    CreateBasicPageController.$inject = ['$scope', 'AccountService', 'PollService'];

    function CreateBasicPageController($scope, AccountService, PollService) {

        $scope.openLoginDialog = function () {
            AccountService.openLoginDialog($scope);
        };

        $scope.openRegisterDialog = function () {
            AccountService.openRegisterDialog($scope);
        };

        $scope.createPoll = function (question) {
            PollService.createPoll(question, createPollSuccessCallback);

            function createPollSuccessCallback(data) {
                window.location.href = "/#/Manage/" + data.ManageId;
            };
        };

    };
})();
