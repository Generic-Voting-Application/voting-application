(function () {
    'use strict';

    angular
        .module('GVA.Voting')
        .controller('PointsVoteController', PointsVoteController);

    PointsVoteController.$inject = ['$scope', '$routeParams', 'ngDialog'];

    function PointsVoteController($scope, $routeParams, ngDialog) {

        $scope.addChoice = addChoice;
        $scope.unallocatedPoints = calculateUnallocatedPoints;
        $scope.addPointsDisabled = shouldAddPointsBeDisabled;
        $scope.subtractPointsDisabled = shouldSubtractPointsBeDisabled;
        $scope.notifyChoiceAdded = notifyChoiceAdded;

        $scope.increaseVote = increaseVote;
        $scope.decreaseVote = decreaseVote;
        $scope.unallocatedPointsPercentage = unallocatedPointsPercentage;


        activate();


        function activate() {

            // Register our getVotes strategy with the parent controller
            $scope.setVoteCallback(getVotes);
        }

        function getVotes(choices) {
            return choices
                .filter(function (choice) { return choice.voteValue; })
                .map(function (choice) {
                    return {
                        ChoiceId: choice.Id,
                        VoteValue: choice.voteValue
                    };
                });
        }

        function addChoice() {
            ngDialog.open({
                template: '/Routes/AddChoiceDialog',
                controller: 'AddVoterChoiceDialogController',
                scope: $scope,
                data: { pollId: $scope.pollId }
            });
        }

        function notifyChoiceAdded() {
            $scope.$emit('voterChoiceAddedEvent');
        }

        function calculateUnallocatedPoints() {
            var unallocatedPoints = $scope.poll.MaxPoints;

            for (var i = 0; i < $scope.poll.Choices.length; i++) {
                unallocatedPoints -= $scope.poll.Choices[i].voteValue || 0;
            }

            return unallocatedPoints;
        }

        function shouldSubtractPointsBeDisabled(choice) {
            return choice.voteValue <= 0;
        }

        function shouldAddPointsBeDisabled(choice) {
            return choice.voteValue >= $scope.poll.MaxPerVote || $scope.unallocatedPoints() === 0;
        }

        function increaseVote(choice) {
            if (choice.voteValue < $scope.poll.MaxPerVote) {
                choice.voteValue = choice.voteValue + 1;
            }
        }

        function decreaseVote(choice) {
            if (choice.voteValue > 0) {
                choice.voteValue = choice.voteValue - 1;
            }
        }

        function unallocatedPointsPercentage() {


            return (calculateUnallocatedPoints() / $scope.poll.MaxPoints) * 100;
        }
    }
})();
