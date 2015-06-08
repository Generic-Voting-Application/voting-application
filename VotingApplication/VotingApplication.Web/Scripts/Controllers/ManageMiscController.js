(function () {
    'use strict';

    angular
        .module('GVA.Creation')
        .controller('ManageMiscController', ManageMiscController);

    ManageMiscController.$inject = ['$scope', '$routeParams', '$location', 'ManageService', 'RoutingService'];


    function ManageMiscController($scope, $routeParams, $location, ManageService, RoutingService) {

        $scope.poll = ManageService.poll;
        $scope.manageId = $routeParams.manageId;

        $scope.updatePoll = updatePoll;
        $scope.return = navigateToManagePage;

        activate();

        function updatePoll() {
            var miscConfig = {
                InviteOnly: $scope.poll.InviteOnly,
                NamedVoting: $scope.poll.NamedVoting,
                ChoiceAdding: $scope.poll.ChoiceAdding,
                IsElectionMode: $scope.poll.IsElectionMode
            };

            ManageService.updatePollMisc($routeParams.manageId, miscConfig)
            .then(navigateToManagePage);
        }

        function navigateToManagePage() {
            RoutingService.navigateToManagePage($scope.manageId);
        }

        function activate() {
            ManageService.registerPollObserver(function () {
                $scope.poll = ManageService.poll;
            });
        }
    }
})();
