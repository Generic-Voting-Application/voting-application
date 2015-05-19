(function () {
    'use strict';

    angular
        .module('GVA.Common')
        .controller('AddOptionDialogController', AddOptionDialogController);

    AddOptionDialogController.$inject = ['$scope'];

    function AddOptionDialogController($scope) {

        $scope.multipleAddingAllowed = true;

        $scope.addOption = addOption;
        $scope.addOptionAndClose = addOptionAndClose;
        $scope.addAnotherToggle = true;

        function addOption(form) {
            if (form.name === null) {
                return;
            }

            var newOption = {
                Name: form.name,
                Description: form.description,
                OptionNumber: null
            };

            form.name = null;
            form.description = null;

            $scope.options.push(newOption);
        }

        function addOptionAndClose(form) {
            addOption(form);

            dismiss();
        }

        function dismiss() {
            $scope.closeThisDialog();
        }
    }
})();