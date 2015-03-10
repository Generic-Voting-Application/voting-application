(function () {
    angular
        .module('GVA.Creation')
        .controller('ManageVotersController', ManageVotersController);

    ManageVotersController.$inject = ['$scope', '$routeParams', '$location', 'ManageService'];


    function ManageVotersController($scope, $routeParams, $location, ManageService) {

        $scope.poll = ManageService.poll;
        $scope.manageId = $routeParams.manageId;

        $scope.updatePoll = updatePoll;
        $scope.return = returnToManage;

        activate();

        function updatePoll() {
            ManageService.updatePoll($routeParams.manageId, $scope.poll, function () {
                ManageService.getPoll($scope.manageId);
            });
        }

        function returnToManage() {
            $location.path('Manage/' + $scope.manageId);
        }

        function activate() {
            ManageService.registerPollObserver(function () {
                $scope.poll = ManageService.poll;
            })
        }
    };
})();
