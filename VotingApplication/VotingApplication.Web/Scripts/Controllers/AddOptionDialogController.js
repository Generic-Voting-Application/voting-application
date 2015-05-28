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
            
            $scope.choices.push(newChoice);

            var formElement = angular.element(document.querySelector('#addChoiceDialog-Form'));
            formElement.attr('novalidate', '');

            form.name = null;
            form.description = null;
            form.reset();

            formElement.removeAttr('novalidate', '');
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