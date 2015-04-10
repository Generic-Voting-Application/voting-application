/// <reference path="../Services/ManageService.js" />
/// <reference path="../Services/RoutingService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Creation')
        .controller('ManageOptionController', ManageOptionController);

    ManageOptionController.$inject = ['$scope', '$routeParams', '$location', 'ManageService', 'RoutingService', 'ngDialog'];

    function ManageOptionController($scope, $routeParams, $location, ManageService, RoutingService, ngDialog) {

        var manageId = $routeParams.manageId;

        $scope.options = [];

        $scope.addOption = addOption;
        $scope.editOption = editOption;
        $scope.removeOption = removeOption;

        $scope.saveOptionsAndReturn = saveOptionsAndReturn;
        $scope.returnWithoutSave = returnWithoutSave;


        activate();

        function activate() {
            //ManageService.registerPollObserver(loadOptions);

            loadOptions();
        }

        function loadOptions() {
            ManageService.getOptions(manageId)
                .then(function (data) {
                    $scope.options = data;
                });
        }

        function addOption() {
            ngDialog.open({
                template: '/Routes/AddOptionDialog',
                controller: 'AddOptionDialogController',
                scope: $scope
            });
        }

        function editOption(option) {
            ngDialog.open({
                template: '/Routes/EditOptionDialog',
                controller: 'EditOptionDialogController',
                scope: $scope,
                data: { option: option }
            });
        }

        function removeOption(option) {
            $scope.options.splice($scope.options.indexOf(option), 1);
        }

        function returnWithoutSave() {
            RoutingService.navigateToManagePage(manageId);
        }

        function saveOptionsAndReturn() {
            ManageService.updateOptions(manageId, $scope.options)
                .then(function () { RoutingService.navigateToManagePage(manageId); });
        }
    }

})();
