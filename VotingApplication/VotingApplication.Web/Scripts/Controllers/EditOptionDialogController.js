(function () {
    'use strict';

    angular
        .module('GVA.Common')
        .controller('EditOptionDialogController', EditOptionDialogController);

    EditOptionDialogController.$inject = ['$scope'];

    function EditOptionDialogController($scope) {
        $scope.updateOption = updateOption;
        $scope.dismiss = dismiss;

        $scope.name = $scope.ngDialogData.option.Name;
        $scope.description = $scope.ngDialogData.option.Description;

        function updateOption() {
            if ($scope.name === null) {
                return;
            }

            $scope.ngDialogData.option.Name = $scope.name;
            $scope.ngDialogData.option.Description = $scope.description;

            dismiss();
        }

        function dismiss() {
            $scope.closeThisDialog();
        }
    }
})();