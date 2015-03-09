(function () {
    angular
        .module('GVA.Creation')
        .controller('ManageVotersController', ['$scope', '$routeParams', '$location', 'ManageService',
        function ($scope, $routeParams, $location, ManageService) {

            $scope.poll = ManageService.poll;
            $scope.manageId = $routeParams.manageId;

            $scope.updatePoll = function () {
                ManageService.updatePoll($routeParams.manageId, $scope.poll, function () {
                    ManageService.getPoll($scope.manageId);
                });
            }

            $scope.return = function () {
                $location.path('Manage/' + $scope.manageId);
            }

            ManageService.registerPollObserver(function () {
                $scope.poll = ManageService.poll;
            })
        }]);
})();
