(function () {
    angular
        .module('GVA.Creation')
        .controller('ManagePollTypeController', ManagePollTypeController);

    ManagePollTypeController.$inject = ['$scope', '$routeParams', '$location', 'ngDialog', 'ManageService', 'RoutingService'];

    function ManagePollTypeController($scope, $routeParams, $location, ngDialog, ManageService, RoutingService) {

        $scope.poll = ManageService.poll;
        $scope.manageId = $routeParams.manageId;

        $scope.updatePoll = updatePoll;
        $scope.return = navigateToManagePage;
        $scope.updateStrategy = updateStrategy;
        $scope.canIncrementMPV = canIncrementMPV;
        $scope.canDecrementMPV = canDecrementMPV;
        $scope.canIncrementMP = canIncrementMP;
        $scope.canDecrementMP = canDecrementMP;

        activate();

        function updatePoll() {
            ManageService.updatePoll($routeParams.manageId, $scope.poll, navigateToManagePage);
        }

        function navigateToManagePage() {
            RoutingService.navigateToManagePage($scope.manageId);
        }

        function updateStrategy(strategy) {

            ManageService.getVotes($scope.poll.UUID, function (votes) {
                if (votes.length > 0) {
                    openPollChangeDialog(function () {
                        $scope.poll.VotingStrategy = strategy;
                    });
                } else {
                    $scope.poll.VotingStrategy = strategy;

                    switch (strategy) {
                        case 'Basic':
                            $scope.poll.MaxPerVote = 1;
                            $scope.poll.MaxPoints = 1;
                            break;
                        case 'Points':
                            $scope.poll.MaxPerVote = 3;
                            $scope.poll.MaxPoints = 7;
                            break;
                        case 'UpDown':
                            $scope.poll.MaxPerVote = 1;
                            $scope.poll.MaxPoints = $scope.poll.Options ? $scope.poll.Options.length : 1;
                            break;
                        case 'Multi':
                            $scope.poll.MaxPerVote = 1;
                            $scope.poll.MaxPoints = $scope.poll.Options ? $scope.poll.Options.length : 1;
                            break;
                    }
                }
            });
        }

        // Points per vote
        function incrementMPV() {
            if (canIncrementMPV) {
                $scope.poll.MaxPerVote++;
            }
        }

        function decrementMPV() {
            if (canDecrementMPV) {
                $scope.poll.MaxPerVote--;
            }
        }

        function canIncrementMPV() {
            if (!$scope.poll) {
                return false;
            }

            return $scope.poll.VotingStrategy === 'Points' && $scope.poll.MaxPerVote < $scope.poll.MaxPoints;
        }

        function canDecrementMPV() {
            if (!$scope.poll) {
                return false;
            }

            return $scope.poll.VotingStrategy === 'Points' && $scope.poll.MaxPerVote > 1;
        }

        // Max points
        function incrementMP() {
            if (canIncrementMP) {
                $scope.poll.MaxPoints++;
            }
        }

        function decrementMP() {
            if (canDecrementMP) {
                $scope.poll.MaxPoints--;
            }
        }

        function canIncrementMP() {
            if (!$scope.poll) {
                return false;
            }

            return $scope.poll.VotingStrategy === 'Points' ||
                   $scope.poll.VotingStrategy === 'UpDown' ||
                   $scope.poll.VotingStrategy === 'Multi';
        }

        function canDecrementMP() {
            if (!$scope.poll) {
                return false;
            }

            var maxPoints = $scope.poll.MaxPoints;
            var maxPerVote = $scope.poll.MaxPerVote;

            switch ($scope.poll.VotingStrategy) {
                case 'Basic':
                    return false;
                case 'Points':
                    return maxPoints > 1 && maxPoints > maxPerVote;
                case 'UpDown':
                case 'Multi':
                    return maxPoints > 1;
                default:
                    return false;
            }
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
            });

            ManageService.getPoll($scope.manageId);
        }
    }
})();
