/// <reference path="../Services/ManageService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Creation')
        .controller('ManageOptionController', ManageOptionController);

    ManageOptionController.$inject = ['$scope', '$routeParams', '$location', 'ManageService', 'RoutingService', 'ngDialog'];

    function ManageOptionController($scope, $routeParams, $location, ManageService, RoutingService, ngDialog) {

        $scope.poll = ManageService.poll;
        $scope.manageId = $routeParams.manageId;
        $scope.updatePoll = updatePollDetails;
        $scope.return = navigateToManagePage;
        $scope.remove = removePollOption;
        $scope.add = addPollOption;
        $scope.edit = editPollOption;

        activate();

        function activate() {
            ManageService.registerPollObserver(function () {
                $scope.poll = ManageService.poll;
            });
        }

        function navigateToManagePage() {
            RoutingService.navigateToManagePage($scope.manageId);
        }

        function removePollOption(option) {
            $scope.poll.Options.splice($scope.poll.Options.indexOf(option), 1);
        }

        function addPollOption() {
            ngDialog.open({
                template: '/Routes/AddOptionDialog',
                controller: 'AddOptionDialogController',
                scope: $scope
            });
        }

        function editPollOption(option) {
            ngDialog.open({
                template: '/Routes/EditOptionDialog',
                controller: 'EditOptionDialogController',
                scope: $scope,
                data: { option: option }
            });
        }

        function updatePollDetails() {
            ManageService.updatePoll($routeParams.manageId, $scope.poll, function () {
                ManageService.getPoll($scope.manageId);
                navigateToManagePage();
            });
        }
    }

})();
