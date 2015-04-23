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
                OptionAdding: $scope.poll.OptionAdding,
                HiddenResults: $scope.poll.HiddenResults
            };

            ManageService.updatePollMisc($routeParams.manageId, miscConfig, navigateToManagePage);
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
