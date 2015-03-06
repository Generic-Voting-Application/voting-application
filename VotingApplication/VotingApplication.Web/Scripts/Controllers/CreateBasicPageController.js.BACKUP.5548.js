(function () {
    angular
        .module('GVA.Creation')
<<<<<<< HEAD
        .controller('CreateBasicPageController', ['$scope', 'AccountService', 'PollService', function ($scope, AccountService, PollService) {

            $scope.openLoginDialog = function () {
                AccountService.openLoginDialog($scope);
            };

            $scope.openRegisterDialog = function () {
                AccountService.openRegisterDialog($scope);
            };

            $scope.createPoll = function (question) {
                PollService.createPoll(question, function (data) {
                    window.location.href = "/Manage/" + data.ManageId;
                });
            };

=======
        .controller('CreateBasicPageController', ['$scope', 'AccountService',
        function ($scope, AccountService) {

            $scope.openLoginDialog = function () {
                AccountService.openLoginDialog($scope);
            }

            $scope.openRegisterDialog = function () {
                AccountService.openRegisterDialog($scope);
            }

>>>>>>> Generic-Voting-Application/feature/new-ux
        }]);
})();
