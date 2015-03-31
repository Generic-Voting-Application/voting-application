/// <reference path="../Services/ManageService.js" />
/// <reference path="../Services/RoutingService.js" />
(function () {
    angular
        .module('GVA.Creation')
        .controller('ManageVotersController', ManageVotersController);

    ManageVotersController.$inject = ['$scope', '$routeParams', '$location', 'ManageService', 'RoutingService'];


    function ManageVotersController($scope, $routeParams, $location, ManageService, RoutingService) {

        var manageId = $routeParams.manageId;

        $scope.voters = {};

        $scope.removeAllVotes = removeAllVotes;

        $scope.removeVote = removeVote;
        $scope.removeBallot = removeBallot;

        $scope.returnWithoutDelete = returnWithoutDelete;
        $scope.confirmDeleteAndReturn = confirmDeleteAndReturn;

        activate();

        function activate() {
            loadVoters();
        }

        function removeAllVotes() {
        }

        function removeVote(vote, ballot) {
        }

        function removeBallot(ballot) {
        }

        function loadVoters() {
            ManageService.getVoters(manageId)
                .success(function (data) {
                    $scope.voters = data;
                });
        }

        function returnWithoutDelete() {
            RoutingService.navigateToManagePage(manageId);
        }

        function confirmDeleteAndReturn() {
            RoutingService.navigateToManagePage(manageId);
        }
    }
})();
