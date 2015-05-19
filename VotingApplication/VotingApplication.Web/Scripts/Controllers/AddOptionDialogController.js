(function () {
    'use strict';

    angular
        .module('GVA.Common')
        .controller('AddChoiceDialogController', AddChoiceDialogController);

    AddChoiceDialogController.$inject = ['$scope'];

    function AddChoiceDialogController($scope) {

        $scope.multipleAddingAllowed = true;

        $scope.addChoice = addChoice;
        $scope.addChoiceAndClose = addChoiceAndClose;
        $scope.addAnotherToggle = true;

        function addChoice(form) {
            if (form.name === null) {
                return;
            }

            var newChoice = {
                Name: form.name,
                Description: form.description,
                ChoiceNumber: null
            };

            form.name = null;
            form.description = null;

            $scope.choices.push(newChoice);
        }

        function addChoiceAndClose(form) {
            addChoice(form);

            dismiss();
        }

        function dismiss() {
            $scope.closeThisDialog();
        }
    }
})();