(function () {
    'use strict';

    angular
        .module('GVA.Voting')
        .controller('PointsVoteController', PointsVoteController);

    PointsVoteController.$inject = ['$scope', '$routeParams', 'ngDialog'];

    function PointsVoteController($scope, $routeParams, ngDialog) {

        $scope.addOption = addOption;
        $scope.unallocatedPoints = calculateUnallocatedPoints;
        $scope.addPointsDisabled = shouldAddPointsBeDisabled;
        $scope.subtractPointsDisabled = shouldSubtractPointsBeDisabled;
        $scope.notifyOptionAdded = notifyOptionAdded;

        $scope.increaseVote = increaseVote;
        $scope.decreaseVote = decreaseVote;
        $scope.unallocatedPointsPercentage = unallocatedPointsPercentage;


        activate();


        function activate() {

            // Register our getVotes strategy with the parent controller
            $scope.setVoteCallback(getVotes);
        }

        function getVotes(options) {
            return options
                .filter(function (option) { return option.voteValue; })
                .map(function (option) {
                    return {
                        OptionId: option.Id,
                        VoteValue: option.voteValue
                    };
                });
        }

        function addOption() {
            ngDialog.open({
                template: '/Routes/AddOptionDialog',
                controller: 'AddVoterOptionDialogController',
                scope: $scope,
                data: { pollId: $scope.pollId }
            });
        }

        function notifyOptionAdded() {
            $scope.$emit('voterOptionAddedEvent');
        }

        function calculateUnallocatedPoints() {
            var unallocatedPoints = $scope.poll.MaxPoints;

            for (var i = 0; i < $scope.poll.Options.length; i++) {
                unallocatedPoints -= $scope.poll.Options[i].voteValue || 0;
            }

            return unallocatedPoints;
        }

        function shouldSubtractPointsBeDisabled(option) {
            return option.voteValue <= 0;
        }

        function shouldAddPointsBeDisabled(option) {
            return option.voteValue >= $scope.poll.MaxPerVote || $scope.unallocatedPoints() === 0;
        }

        function increaseVote(option) {
            if (option.voteValue < $scope.poll.MaxPerVote) {
                option.voteValue = option.voteValue + 1;
            }
        }

        function decreaseVote(option) {
            if (option.voteValue > 0) {
                option.voteValue = option.voteValue - 1;
            }
        }

        function unallocatedPointsPercentage() {


            return (calculateUnallocatedPoints() / $scope.poll.MaxPoints) * 100;
        }
    }
})();
