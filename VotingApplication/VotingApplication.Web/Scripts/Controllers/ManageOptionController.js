/// <reference path="../Services/ManageService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Creation')
        .controller('ManageOptionController', ManageOptionController);

    ManageOptionController.$inject = ['$scope', '$routeParams', '$location', 'ManageService', 'RoutingService'];

    function ManageOptionController($scope, $routeParams, $location, ManageService, RoutingService) {

        $scope.poll = ManageService.poll;
        $scope.manageId = $routeParams.manageId;
        $scope.updatePoll = updatePollDetails;
        $scope.return = navigateToManagePage;
        $scope.remove = removePollOption;
        $scope.add = addPollOption;
        $scope.catchDirtyInput = catchDirtyInput;

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

        function catchDirtyInput() {
            if ($scope.newOptionForm.$dirty && $scope.newOptionForm.$valid) {
                addPollOption($scope.newOptionForm);
            }
        }

        function updatePollDetails() {
            ManageService.updatePoll($routeParams.manageId, $scope.poll, function () {
                ManageService.getPoll($scope.manageId);
                navigateToManagePage();
            });
        }
    }

})();
