(function () {
    'use strict';

    angular
        .module('VoteOn-Vote')
        .controller('AddUserChoiceDialogController', AddUserChoiceDialogController);

    AddUserChoiceDialogController.$inject = ['$scope', '$mdDialog'];


    function AddUserChoiceDialogController($scope, $mdDialog) {

        $scope.userChoice = null;

        $scope.closeDialog = closeDialog;
        $scope.addChoice = addChoice;

        function closeDialog() {
            $mdDialog.cancel();
        }

        function addChoice() {
            if ($scope.addUserChoiceForm.$valid) {
                $mdDialog.hide($scope.userChoice);
            }
        }
    }
})();