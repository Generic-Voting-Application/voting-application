/// <reference path="../Services/VoteService.js" />
'use strict';

(function () {
    angular
        .module('GVA.Voting')
        .controller('AddVoterOptionDialogController', AddVoterOptionDialogController);

    AddVoterOptionDialogController.$inject = ['$scope', 'VoteService'];

    function AddVoterOptionDialogController($scope, VoteService) {

        $scope.multipleAddingAllowed = false;

        $scope.addOption = addOption;
        $scope.addOptionAndClose = addOptionAndClose;

        function addOption(form) {
            add(form);
            dismiss();
        }

        function add(form) {
            if (form.name === null) {
                return;
            }

            var newVoterOption = {
                Name: form.name,
                Description: form.description
            };

            $scope.options.push(newVoterOption);

            VoteService.addVoterOption($scope.ngDialogData.pollId, newVoterOption)
                .then($scope.notifyOptionAdded);
        }

        function addOptionAndClose(form) {
            add(form);
            dismiss();
        }

        function dismiss() {
            $scope.closeThisDialog();
        }
    }
})();