/// <reference path="../Services/AccountService.js" />
/// <reference path="../Services/ManageService.js" />
(function () {
    angular
        .module('GVA.Creation')
        .controller('ManagePageController', ManagePageController);

    ManagePageController.$inject = ['$scope', '$routeParams', 'AccountService', 'ManageService'];

    function ManagePageController($scope, $routeParams, AccountService, ManageService) {

        var manageId = $routeParams.manageId;

        $scope.poll = {};
        $scope.manageId = manageId;

        $scope.openLoginDialog = showLoginDialog;
        $scope.updatePoll = updatePollDetails;
        $scope.formatPollExpiry = formatPollExpiryDate;
        $scope.selectText = selectTargetText;

        activate();


        function activate() {
            ManageService.getPoll(manageId, function (data) {
                $scope.poll = data;
            });
        }

        function showLoginDialog() {
            AccountService.openLoginDialog($scope);
        };

        function updatePollDetails() {
            ManageService.poll = $scope.poll;
            ManageService.updatePoll($routeParams.manageId, $scope.poll);
        };

        function formatPollExpiryDate() {
            if (!$scope.poll.Expires || !$scope.poll.ExpiryDate) {
                return 'Never';
            }

            var expiryDate = new Date($scope.poll.ExpiryDate);
            return expiryDate.toLocaleString();
        };

        function selectTargetText($event) {
            $event.target.select();
        };
    }
})();
