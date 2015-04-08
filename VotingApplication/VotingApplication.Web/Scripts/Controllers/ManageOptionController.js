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
        $scope.openAddOptionDialog = openAddOptionDialog;

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

        function clearPollOption(form) {
            form.Name = '';
            form.Description = '';
            form.$setPristine();
        }

        function addPollOption(optionForm) {
            var newOption = {
                Name: optionForm.Name,
                Description: optionForm.Description
            };

            $scope.poll.Options.push(newOption);

            clearPollOption(optionForm);
        }

        function editPollOption(option) {
            $scope.poll.Options.forEach(function (d) {
                d.editOptionToggle = (option === d);
            });
        }

        function openAddOptionDialog() {
            ngDialog.open({
                template: '/Routes/AddOptionDialog',
                controller: 'AddOptionDialogController',
                'scope': $scope
                //data: { 'callback': callback }
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
