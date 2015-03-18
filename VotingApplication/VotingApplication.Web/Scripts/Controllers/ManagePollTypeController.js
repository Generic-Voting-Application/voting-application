(function () {
    angular
        .module('GVA.Creation')
        .controller('ManagePollTypeController', ManageVotersController);

    ManageVotersController.$inject = ['$scope', '$routeParams', '$location', 'ngDialog', 'ManageService'];

    function ManageVotersController($scope, $routeParams, $location, ngDialog, ManageService) {

        $scope.poll = ManageService.poll;
        $scope.manageId = $routeParams.manageId;

        $scope.updatePoll = updatePoll;
        $scope.return = returnToManage;
        $scope.updateStrategy = updateStrategy;

        activate();

        function updatePoll() {
            ManageService.updatePoll($routeParams.manageId, $scope.poll, function () {
                ManageService.getPoll();
            });
        }

        function returnToManage() {
            $location.path('Manage/' + $scope.manageId);
        }

        function updateStrategy(strategy) {

            ManageService.getVotes($scope.poll.UUID, function (votes) {
                if (votes.length > 0) {
                    openPollChangeDialog(function () {
                        $scope.poll.VotingStrategy = strategy;
                        updatePoll();
                    });
                } else {
                    $scope.poll.VotingStrategy = strategy;
                    updatePoll();
                }
            });
        }

        function activate() {
            ManageService.registerPollObserver(function () {
                $scope.poll = ManageService.poll;
            });

            ManageService.getPoll($scope.manageId);
        }

        function openPollChangeDialog(callback) {
            ngDialog.open({
                template: '../Routes/PollTypeChange',
                controller: 'PollTypeChangeController',
                'scope': $scope,
                data: { 'callback': callback }
            });
        }
    };
})();
