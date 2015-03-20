(function () {
    angular
        .module('GVA.Creation')
        .controller('ManageInvitationsController', ManageInvitationsController);

    ManageInvitationsController.$inject = ['$scope', '$routeParams', '$location', 'ManageService'];


    function ManageInvitationsController($scope, $routeParams, $location, ManageService) {

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
            ManageService.registerPollObserver(function() {
                $scope.poll = ManageService.poll;
            });
        }
    }
})();
