(function () {
    angular.module('GVA.Creation').controller('ManageNameController', ['$scope', '$routeParams', '$location', 'ManageService', function ($scope, $routeParams, $location, ManageService) {

        $scope.poll = ManageService.poll;
        $scope.manageId = $routeParams.manageId;

        $scope.return = function () {
            $location.path('Manage/' + $scope.manageId);
        }

        $scope.update = function () {
            ManageService.poll = $scope.poll;
            ManageService.updatePoll($scope.return);
        }

        ManageService.registerPollObserver(function () {
            $scope.poll = ManageService.poll;
        })
    }]);
})();
