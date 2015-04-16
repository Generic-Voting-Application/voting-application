/// <reference path="../Services/IdentityService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Voting')
        .controller('PointsVoteController', PointsVoteController);

    PointsVoteController.$inject = ['$scope', '$routeParams', 'IdentityService', 'ngDialog'];

    function PointsVoteController($scope, $routeParams, IdentityService, ngDialog) {

        $scope.addOption = addOption;
        $scope.unallocatedPoints = calculateUnallocatedPoints;
        $scope.disabledAddPoints = shouldAddPointsBeDisabled;
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
                        VoteValue: option.voteValue,
                        VoterName: IdentityService.identity && $scope.poll && $scope.poll.NamedVoting ?
                                   IdentityService.identity.name : null
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

        function shouldAddPointsBeDisabled(pointValue) {
            return pointValue >= $scope.poll.MaxPerVote || $scope.unallocatedPoints() === 0;
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
