(function () {
    'use strict';

    angular
        .module('GVA.Common')
        .controller('EditOptionDialogController', EditOptionDialogController);

    EditOptionDialogController.$inject = ['$scope'];

    function EditOptionDialogController($scope) {
        $scope.updateOption = updateOption;

        $scope.editOptionFormName = $scope.ngDialogData.option.Name;
        $scope.editOptionFormDescription = $scope.ngDialogData.option.Description;

        function updateOption() {
            if ($scope.name === null) {
                return;
            }

            $scope.ngDialogData.option.Name = $scope.editOptionFormName;
            $scope.ngDialogData.option.Description = $scope.editOptionFormDescription;

            $scope.closeThisDialog();
        }
    }
})();