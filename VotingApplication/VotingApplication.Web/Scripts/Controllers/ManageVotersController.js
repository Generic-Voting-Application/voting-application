/// <reference path="../Services/ManageService.js" />
/// <reference path="../Services/RoutingService.js" />
(function () {
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
            $scope.voters.forEach(function (ballotToRemove) {

                var existingVoterToRemove = $scope.votersToRemove.filter(filterBallotByGuid(ballotToRemove));

                if (existingVoterToRemove.length === 0) {
                    $scope.votersToRemove.push(ballotToRemove);
                }
                else {
                    // Existing ballot, add votes to it.

                    ballotToRemove.Votes.forEach(function (vote) {

                        console.log(vote);
                        var votes = existingVoterToRemove[0].Votes.filter(filterVoteByOptionNumber(vote));

                        console.log(votes);

                        if (votes.length === 0) {
                            existingVoterToRemove[0].Votes.push(vote);
                            console.log(existingVoterToRemove[0].Votes);
                        }
                    });
                }
            });


            $scope.voters = [];
        }

        function filterBallotByGuid(item) {
            return function (value) { return value.BallotManageGuid === item.BallotManageGuid; };
        }

        function filterVoteByOptionNumber(item) {
            return function (value) { return value.OptionNumber === item.OptionNumber; };
        }

        function removeVote(vote, ballot) {
        }

        function removeBallot(ballot) {
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
            RoutingService.navigateToManagePage(manageId);
        }
    }
})();
