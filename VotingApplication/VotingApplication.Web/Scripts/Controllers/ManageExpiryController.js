/// <reference path="../Services/ManageService.js" />
(function () {
    angular
        .module('GVA.Creation')
        .controller('ManageExpiryController', ManageOptionController);

    ManageOptionController.$inject = ['$scope', '$routeParams', '$location', 'ManageService'];

    function ManageOptionController($scope, $routeParams, $location, ManageService) {

        $scope.poll = ManageService.poll;
        $scope.manageId = $routeParams.manageId;
        $scope.updatePoll = updatePollDetails;
        $scope.return = navigateToManagePage;
        $scope.formatDate = formatDate;

        activate();
        
        function activate() {
            ManageService.registerPollObserver(function () {
                $scope.poll = ManageService.poll;
            });
        }

        function formatDate() {
            if ($scope.poll){
                return moment($scope.poll.ExpiryDate).format("dddd, MMMM Do YYYY, h:mm:ss a");
            }
            return 'Never';
        }

        function navigateToManagePage() {
            $location.path('Manage/' + $scope.manageId);
        };

        function updatePollDetails() {
            ManageService.updatePoll($routeParams.manageId, $scope.poll, updatePollSuccessCallback);
        };

        function updatePollSuccessCallback() {
            ManageService.getPoll($scope.manageId);
        };
    }

})();
