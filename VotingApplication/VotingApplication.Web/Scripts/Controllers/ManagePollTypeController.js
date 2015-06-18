(function () {
    'use strict';

    angular
        .module('GVA.Manage')
        .controller('ManagePollTypeController', ManagePollTypeController);

    ManagePollTypeController.$inject = ['$scope', '$routeParams', '$location', 'ngDialog', 'ManageService', 'RoutingService'];

    function ManagePollTypeController($scope, $routeParams, $location, ngDialog, ManageService, RoutingService) {

        $scope.poll = ManageService.poll;
        $scope.manageId = $routeParams.manageId;

        $scope.updatePoll = tentativeUpdatePoll;
        $scope.return = navigateToManagePage;
        $scope.updateStrategy = updateStrategy;
        $scope.canIncrementMPV = canIncrementMPV;
        $scope.canDecrementMPV = canDecrementMPV;
        $scope.canIncrementMP = canIncrementMP;
        $scope.canDecrementMP = canDecrementMP;

        var startingPollType = null;
        var startingMaxPerVote = null;
        var startingMaxPoints = null;

        activate();

        function updatePollType() {

            if ($scope.poll.PollType !== 'Points'){
                $scope.poll.MaxPerVote = startingMaxPerVote;
                $scope.poll.MaxPoints = startingMaxPoints;
            }

            var pollTypeConfig = {
                PollType: $scope.poll.PollType,
                MaxPerVote: $scope.poll.MaxPerVote,
                MaxPoints: $scope.poll.MaxPoints
            };

            ManageService.updatePollType($routeParams.manageId, pollTypeConfig)
            .then(navigateToManagePage);
        }

        function tentativeUpdatePoll() {
            ManageService.getVotes($scope.poll.UUID)
            .then(function (pollSummary) {
                if (pollSummary.data && pollSummary.data.Winners && pollSummary.data.Winners.length > 0 &&
                        ($scope.poll.PollType !== startingPollType ||
                        ($scope.poll.PollType === 'Points' &&
                            ($scope.poll.MaxPerVote !== startingMaxPerVote ||
                            $scope.poll.MaxPoints !== startingMaxPoints)))) {
                    openPollChangeDialog(updatePollType);
                } else {
                    updatePollType();
                }
            });
        }

        function navigateToManagePage() {
            RoutingService.navigateToManagePage($scope.manageId);
        }

        function updateStrategy(strategy) {
            $scope.poll.PollType = strategy;
        }

        // Points per vote
        function canIncrementMPV() {
            if (!$scope.poll) {
                return false;
            }

            return $scope.poll.PollType === 'Points' && $scope.poll.MaxPerVote < $scope.poll.MaxPoints;
        }

        function canDecrementMPV() {
            if (!$scope.poll) {
                return false;
            }

            return $scope.poll.PollType === 'Points' && $scope.poll.MaxPerVote > 1;
        }

        // Max points
        function canIncrementMP() {
            if (!$scope.poll) {
                return false;
            }

            return $scope.poll.PollType === 'Points';
        }

        function canDecrementMP() {
            if (!$scope.poll) {
                return false;
            }

            return $scope.poll.PollType === 'Points' && $scope.poll.MaxPoints > 1 && $scope.poll.MaxPoints > $scope.poll.MaxPerVote;
        }

        function openPollChangeDialog(callback) {
            ngDialog.open({
                template: '../Routes/PollTypeChange',
                controller: 'PollTypeChangeController',
                'scope': $scope,
                data: { 'callback': callback }
            });
        }

        function activate() {
            ManageService.registerPollObserver(function () {
                $scope.poll = ManageService.poll;
                $scope.poll.MaxPerVote = $scope.poll.MaxPerVote || 3;
                $scope.poll.MaxPoints = $scope.poll.MaxPoints || 7;
                startingPollType = $scope.poll.PollType;

                startingMaxPerVote = $scope.poll.MaxPerVote;
                startingMaxPoints = $scope.poll.MaxPoints;
            });

            ManageService.getPoll($scope.manageId);
        }
    }
})();
