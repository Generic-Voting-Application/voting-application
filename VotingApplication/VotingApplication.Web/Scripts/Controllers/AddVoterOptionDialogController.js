/// <reference path="../Services/VoteService.js" />
'use strict';

(function () {
    angular
        .module('GVA.Voting')
        .controller('AddVoterChoiceDialogController', AddVoterChoiceDialogController);

    AddVoterChoiceDialogController.$inject = ['$scope', 'VoteService'];

    function AddVoterChoiceDialogController($scope, VoteService) {

        $scope.multipleAddingAllowed = false;

        $scope.addChoice = addChoice;
        $scope.addChoiceAndClose = addChoiceAndClose;
        $scope.addAnotherToggle = false;

        function addChoice(form) {
            add(form);
        }

        function add(form) {
            if (form.name === null) {
                return;
            }

            var newVoterChoice = {
                Name: form.name,
                Description: form.description
            };

            VoteService.addVoterChoice($scope.ngDialogData.pollId, newVoterChoice)
                .then($scope.notifyChoiceAdded);
        }

        function addChoiceAndClose(form) {
            add(form);
            dismiss();
        }

        function dismiss() {
            $scope.closeThisDialog();
        }
    }
})();