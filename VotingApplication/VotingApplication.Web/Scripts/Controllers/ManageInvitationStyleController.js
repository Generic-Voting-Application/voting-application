(function () {
    "use strict";

    angular
        .module('GVA.Creation')
        .controller('ManageInvitationStyleController', ManageInvitationStyleController);

    ManageInvitationStyleController.$inject = ['$scope', '$routeParams', '$location', 'ManageService', 'RoutingService'];


    function ManageInvitationStyleController($scope, $routeParams, $location, ManageService, RoutingService) {

        $scope.poll = ManageService.poll;
        $scope.manageId = $routeParams.manageId;

        $scope.updatePoll = updatePoll;
        $scope.return = navigateToManagePage;

        activate();

        function updatePoll() {
            ManageService.updatePoll($routeParams.manageId, $scope.poll, navigateToManagePage);
        }

        function navigateToManagePage() {
            RoutingService.navigateToManagePage($scope.manageId);
        }

        function activate() {
            ManageService.registerPollObserver(function() {
                $scope.poll = ManageService.poll;
            });
        }
    }
})();
