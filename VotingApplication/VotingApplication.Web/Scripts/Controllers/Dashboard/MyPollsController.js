(function () {
    'use strict';

    angular
        .module('VoteOn-Dashboard')
        .controller('MyPollsController', MyPollsController);

    MyPollsController.$inject = ['$scope', 'PollService', 'RoutingService'];

    function MyPollsController($scope, PollService, RoutingService) {

        $scope.polls = [];
        $scope.loaded = false;

        $scope.navigateToPoll = navigateToPoll;

        activate();

        function activate() {
            PollService.getUserPolls()
            .then(function (data) {
                $scope.loaded = true;
                $scope.polls = data;
            });
        }

        function navigateToPoll(UUID) {
            RoutingService.navigateToVotePage(UUID);
        }
    }
})();
