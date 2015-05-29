(function () {
    'use strict';

    angular
        .module('GVA.Common')
        .controller('EditChoiceDialogController', EditChoiceDialogController);

    EditChoiceDialogController.$inject = ['$scope'];

    function EditChoiceDialogController($scope) {
        $scope.updateChoice = updateChoice;
        
        $scope.setForm = setForm;

        function setForm(form) {
            $scope.editChoiceForm = form;

            $scope.editChoiceForm.name = $scope.ngDialogData.choice.Name;
            $scope.editChoiceForm.description = $scope.ngDialogData.choice.Description;
        }

        function updateChoice() {
            if ($scope.name === null) {
                return;
            }

            $scope.ngDialogData.choice.Name = $scope.editChoiceForm.name;
            $scope.ngDialogData.choice.Description = $scope.editChoiceForm.description;

            $scope.closeThisDialog();
        }
    }
})();