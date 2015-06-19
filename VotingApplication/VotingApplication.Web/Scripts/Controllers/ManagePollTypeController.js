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

            if ($scope.PollType !== 'Points'){
                $scope.MaxPerVote = startingMaxPerVote;
                $scope.MaxPoints = startingMaxPoints;
            }

            var pollTypeConfig = {
                PollType: $scope.PollType,
                MaxPerVote: $scope.MaxPerVote,
                MaxPoints: $scope.MaxPoints
            };

            ManageService.updatePollType($routeParams.manageId, pollTypeConfig)
            .then(navigateToManagePage);
        }

        function tentativeUpdatePoll() {
            ManageService.getPollType($scope.manageId)
            .then(function (data) {

                if (data.PollHasVotes &&
                        ($scope.PollType !== startingPollType ||
                        ($scope.PollType === 'Points' &&
                            ($scope.MaxPerVote !== startingMaxPerVote ||
                            $scope.MaxPoints !== startingMaxPoints)))) {
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
            $scope.PollType = strategy;
        }

        // Points per vote
        function canIncrementMPV() {
            console.log($scope.PollType === 'Points' && $scope.MaxPerVote < $scope.MaxPoints);

            if (!$scope.PollType) {
                return false;
            }

            return $scope.PollType === 'Points' && $scope.MaxPerVote < $scope.MaxPoints;
        }

        function canDecrementMPV() {
            if (!$scope.PollType) {
                return false;
            }

            return $scope.PollType === 'Points' && $scope.MaxPerVote > 1;
        }

        // Max points
        function canIncrementMP() {
            if (!$scope.PollType) {
                return false;
            }

            return $scope.PollType === 'Points';
        }

        function canDecrementMP() {
            if (!$scope.PollType) {
                return false;
            }

            return $scope.PollType === 'Points' && $scope.MaxPoints > 1 && $scope.MaxPoints > $scope.MaxPerVote;
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
            ManageService.getPollType($scope.manageId)
            .then(function (data) {

                $scope.MaxPerVote = data.MaxPerVote || 3;
                $scope.MaxPoints = data.MaxPoints || 7;
                $scope.PollType = data.PollType;
                                
                startingMaxPerVote = data.MaxPerVote;
                startingMaxPoints = data.MaxPoints;
                startingPollType = data.PollType;
            });
        }
    }
})();
