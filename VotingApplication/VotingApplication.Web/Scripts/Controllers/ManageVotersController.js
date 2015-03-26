/// <reference path="../Services/ManageService.js" />
(function () {
    angular
        .module('GVA.Creation')
        .controller('ManageVotersController', ManageVotersController);

    ManageVotersController.$inject = ['$scope', '$routeParams', '$location', 'ManageService', 'RoutingService'];


    function ManageVotersController($scope, $routeParams, $location, ManageService, RoutingService) {

        $scope.voters = {};
        $scope.manageId = $routeParams.manageId;

        activate();

        function activate() {
            ManageService.getVoters($scope.manageId)
            .success(function (data) {
                $scope.voters = data;
            });
        }
    }
})();
