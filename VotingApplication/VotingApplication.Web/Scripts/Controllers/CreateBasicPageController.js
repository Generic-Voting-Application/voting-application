(function () {
    angular
        .module('GVA.Creation')
        .controller('CreateBasicPageController', ['$scope', 'AccountService', 'PollService',
        function ($scope, AccountService, PollService) {

            $scope.openLoginDialog = function () {
                AccountService.openLoginDialog($scope);
            };

            $scope.openRegisterDialog = function () {
                AccountService.openRegisterDialog($scope);
            };

            $scope.createPoll = function (question) {
                PollService.createPoll(question, function (data) {
                    window.location.href = "/#/Manage/" + data.ManageId;
                });
            };

        }]);
})();
