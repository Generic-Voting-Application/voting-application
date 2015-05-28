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

            var formElement = angular.element(document.querySelector('#addChoiceDialog-Form'));
            formElement.attr('novalidate', '');

            form.name = null;
            form.description = null;
            form.reset();

            formElement.removeAttr('novalidate', '');
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