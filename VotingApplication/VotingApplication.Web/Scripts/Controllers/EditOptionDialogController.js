(function () {
    'use strict';

    angular
        .module('GVA.Common')
        .controller('EditChoiceDialogController', EditChoiceDialogController);

    EditChoiceDialogController.$inject = ['$scope'];

    function EditChoiceDialogController($scope) {
        $scope.updateChoice = updateChoice;

        $scope.editChoiceFormName = $scope.ngDialogData.choice.Name;
        $scope.editChoiceFormDescription = $scope.ngDialogData.choice.Description;

        function updateChoice() {
            if ($scope.name === null) {
                return;
            }

            $scope.ngDialogData.choice.Name = $scope.editChoiceFormName;
            $scope.ngDialogData.choice.Description = $scope.editChoiceFormDescription;

            $scope.closeThisDialog();
        }
    }
})();