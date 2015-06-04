/// <reference path="../Services/ManageService.js" />
/// <reference path="../Services/RoutingService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Manage')
        .controller('ManageChoiceController', ManageChoiceController);

    ManageChoiceController.$inject = ['$scope', '$routeParams', '$location', 'ManageService', 'RoutingService', 'ngDialog'];

    function ManageChoiceController($scope, $routeParams, $location, ManageService, RoutingService, ngDialog) {

        var manageId = $routeParams.manageId;

        $scope.choices = [];

        $scope.addChoice = addChoice;
        $scope.editChoice = editChoice;
        $scope.removeChoice = removeChoice;

        $scope.saveChoicesAndReturn = saveChoicesAndReturn;
        $scope.returnWithoutSave = returnWithoutSave;


        activate();

        function activate() {
            //ManageService.registerPollObserver(loadChoices);

            loadChoices();
        }

        function loadChoices() {
            ManageService.getChoices(manageId)
                .then(function (data) {
                    $scope.choices = data;
                });
        }

        function addChoice() {
            ngDialog.open({
                template: '/Routes/AddChoiceDialog',
                controller: 'AddChoiceDialogController',
                scope: $scope
            });
        }

        function editChoice(choice) {
            ngDialog.open({
                template: '/Routes/EditChoiceDialog',
                controller: 'EditChoiceDialogController',
                scope: $scope,
                data: { choice: choice }
            });
        }

        function removeChoice(choice) {
            $scope.choices.splice($scope.choices.indexOf(choice), 1);
        }

        function returnWithoutSave() {
            RoutingService.navigateToManagePage(manageId);
        }

        function saveChoicesAndReturn() {
            ManageService.updateChoices(manageId, $scope.choices)
                .then(function () { RoutingService.navigateToManagePage(manageId); });
        }
    }

})();
