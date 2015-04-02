(function () {
    "use strict";

    angular
        .module('GVA.Creation')
        .controller('PollTypeChangeController', PollTypeChangeController);

    PollTypeChangeController.$inject = ['$scope'];

    function PollTypeChangeController($scope) {

        $scope.confirm = confirm;
        $scope.back = back;

        function confirm() {

            $scope.closeThisDialog();
            if ($scope.ngDialogData.callback) {
                $scope.ngDialogData.callback();
            }
        }

        function back() {
            $scope.closeThisDialog();
        }
    }

})();
