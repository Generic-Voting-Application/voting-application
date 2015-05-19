/// <reference path="../Services/ManageService.js" />
/// <reference path="../Services/RoutingService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Creation')
        .controller('ManageVotersController', ManageVotersController);

    ManageVotersController.$inject = ['$scope', '$routeParams', '$location', 'ManageService', 'RoutingService'];


    function ManageVotersController($scope, $routeParams, $location, ManageService, RoutingService) {

        var manageId = $routeParams.manageId;

        $scope.voters = [];
        $scope.votersToRemove = [];

        $scope.removeAllVotes = removeAllVotes;

        $scope.removeVote = removeVote;
        $scope.removeBallot = removeBallot;

        $scope.returnWithoutDelete = returnWithoutDelete;
        $scope.confirmDeleteAndReturn = confirmDeleteAndReturn;

        activate();

        function activate() {
            loadVoters();
        }

        function removeAllVotes() {
            var clone = $scope.voters.slice(0);

            clone.forEach(removeBallot);
        }

        function removeVote(vote, ballot) {

            var existingBallotToRemove = $scope.votersToRemove.filter(filterBallotByGuid(ballot));

            if (existingBallotToRemove.length === 0) {
                var newBallot = createNewBallotFromExisting(ballot);

                newBallot.Votes.push(vote);

                $scope.votersToRemove.push(newBallot);
            }
            else {
                existingBallotToRemove[0].Votes.push(vote);
            }

            var index = ballot.Votes.indexOf(vote);
            ballot.Votes.splice(index, 1);

            if (ballot.Votes.length === 0) {
                var voteIndex = $scope.voters.indexOf(ballot);
                $scope.voters.splice(voteIndex, 1);
            }
        }

        function createNewBallotFromExisting(ballot) {
            var newBallot = {
                BallotManageGuid: ballot.BallotManageGuid,
                VoterName: ballot.VoterName,
                Votes: []
            };

            return newBallot;
        }

        function removeBallot(ballotToRemove) {
            var existingVoterToRemove = $scope.votersToRemove.filter(filterBallotByGuid(ballotToRemove));

            if (existingVoterToRemove.length === 0) {
                $scope.votersToRemove.push(ballotToRemove);
            }
            else {
                ballotToRemove.Votes.forEach(function (vote) {
                    var votes = existingVoterToRemove[0].Votes.filter(filterVoteByChoiceNumber(vote));

                    if (votes.length === 0) {
                        existingVoterToRemove[0].Votes.push(vote);
                    }
                });
            }

            var index = $scope.voters.indexOf(ballotToRemove);
            $scope.voters.splice(index, 1);
        }

        function filterBallotByGuid(item) {
            return function (value) { return value.BallotManageGuid === item.BallotManageGuid; };
        }

        function filterVoteByChoiceNumber(item) {
            return function (value) { return value.ChoiceNumber === item.ChoiceNumber; };
        }

        function loadVoters() {
            ManageService.getVoters(manageId)
                .then(function (data) {
                    $scope.voters = data;
                });
        }

        function returnWithoutDelete() {
            RoutingService.navigateToManagePage(manageId);
        }

        function confirmDeleteAndReturn() {
            ManageService.deleteVoters(manageId, $scope.votersToRemove)
                .then(function () { RoutingService.navigateToManagePage(manageId); });
        }
    }
})();
