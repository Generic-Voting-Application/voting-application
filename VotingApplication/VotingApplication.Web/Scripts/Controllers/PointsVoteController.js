/// <reference path="../Services/IdentityService.js" />
/// <reference path="../Services/PollService.js" />
/// <reference path="../Services/TokenService.js" />
/// <reference path="../Services/VoteService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Voting')
        .controller('PointsVoteController', PointsVoteController);

    PointsVoteController.$inject = ['$scope', '$routeParams', 'IdentityService', 'PollService', 'TokenService', 'VoteService', 'ngDialog'];

    function PointsVoteController($scope, $routeParams, IdentityService, PollService, TokenService, VoteService, ngDialog) {

        var pollId = $routeParams.pollId;
        var token = null;

        $scope.options = [];
        $scope.totalPointsAvailable = 0;
        $scope.maxPointsPerOption = 0;
        $scope.optionAddingAllowed = false;

        $scope.addOption = addOption;
        $scope.unallocatedPoints = calculateUnallocatedPoints;
        $scope.disabledAddPoints = shouldAddPointsBeDisabled;
        $scope.notifyOptionAdded = notifyOptionAdded;

        // Register our getVotes strategy with the parent controller
        $scope.setVoteCallback(getVotes);

        activate();


        function activate() {
            $scope.$watch('poll', function () {
                $scope.options = $scope.poll ? $scope.poll.Options : [];

                $scope.options.forEach(function (d) {
                    d.voteValue = 0;
                });

                $scope.totalPointsAvailable = $scope.poll ? $scope.poll.MaxPoints : 0;
                $scope.maxPointsPerOption = $scope.poll ? $scope.poll.MaxPerVote : 0;
            });



            $scope.optionAddingAllowed = $scope.poll.OptionAdding;

            TokenService.getToken(pollId, getTokenSuccessCallback);
        }

        function getTokenSuccessCallback(tokenData) {
            token = tokenData;


            VoteService.getTokenVotes(pollId, token, function (voteData) {

                voteData.forEach(function (dataItem) {

                    for (var i = 0; i < $scope.options.length; i++) {
                        var option = $scope.options[i];

                        if (option.Id === dataItem.OptionId) {
                            option.voteValue = dataItem.VoteValue;
                            break;
                        }
                    }
                });
            });
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
                data: { pollId: pollId }
            });
        }

        function notifyOptionAdded() {
            $scope.$emit('voterOptionAddedEvent');
        }

        function calculateUnallocatedPoints() {
            var unallocatedPoints = $scope.totalPointsAvailable;

            for (var i = 0; i < $scope.options.length; i++) {
                unallocatedPoints -= $scope.options[i].voteValue || 0;
            }

            return unallocatedPoints;
        }

        function shouldAddPointsBeDisabled(pointValue) {
            return pointValue >= $scope.maxPointsPerOption || $scope.unallocatedPoints() === 0;
        }
    }
})();
