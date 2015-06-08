/// <reference path="../Services/IdentityService.js" />
/// <reference path="../Services/VoteService.js" />
/// <reference path="../Services/TokenService.js" />
/// <reference path="../Services/RoutingService.js" />
/// <reference path="../Services/PollService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Voting')
        .controller('VotingPageController', VotingPageController);


    VotingPageController.$inject = ['$scope', '$routeParams', 'IdentityService', 'VoteService', 'TokenService', 'RoutingService', 'PollService'];

    function VotingPageController($scope, $routeParams, IdentityService, VoteService, TokenService, RoutingService, PollService) {

        $scope.pollId = $routeParams['pollId'];
        $scope.token = $routeParams['tokenId'] || '';
        $scope.manageId = '';

        $scope.poll = { Choices: [] };
        $scope.resultsLink = RoutingService.getResultsPageUrl($scope.pollId, $scope.token);
        $scope.manageLink = null;

        $scope.identityName = IdentityService.identity ? IdentityService.identity.name : null;
        $scope.logoutIdentity = IdentityService.clearIdentityName;
        $scope.gvaExpiredCallback = redirectIfExpired;
        $scope.submitVote = submitVote;
        $scope.clearVote = clearVote;        

        var getVotes = function () { return []; };
        $scope.setVoteCallback = function (votesFunc) { getVotes = votesFunc; };

        activate();

        function activate() {

            $scope.$on('voterChoiceAddedEvent', choiceAdded);

            // Angular won't auto update this so we need to use the observer pattern
            IdentityService.registerIdentityObserver(function () {
                $scope.identityName = IdentityService.identity ? IdentityService.identity.name : null;
            });

            getManageLink();
            getToken();
        }

        function getToken() {
            TokenService.getToken($scope.pollId)
            .then(function (tokenData) { $scope.token = tokenData; })
            .then(getPollData);
        }

        function getManageLink() {
            TokenService.getManageId($scope.pollId)
            .then(function (manageData) {
                $scope.manageLink = RoutingService.getManagePageUrl(manageData);
            });
        }

        function getPollData() {
            PollService.getPoll($scope.pollId)
                .then(function (response) {

                    $scope.poll = response.data;

                    setSelectedValues();
                });
        }
        
        function setSelectedValues() {
            VoteService.getTokenVotes($scope.pollId, $scope.token)
            .then(function (response) {

                var voteData = response.data;

                $scope.poll.Choices.forEach(function (opt) { opt.voteValue = 0; });
                
                if (voteData) {
                    if ($scope.poll.IsElectionMode && voteData.length > 0) {
                        RoutingService.navigateToResultsPage($scope.pollId, $scope.token);
                    }

                    voteData.forEach(function (vote) {
                        $scope.poll.Choices.forEach(function (choice) {
                            if (choice.Id === vote.ChoiceId) {
                                choice.voteValue = vote.VoteValue;
                            }
                        });
                    });
                }
            });
        }

        function choiceAdded() {

            var currentlySelectedChoices = $scope.poll.Choices.filter(function (opt) {
                return opt.voteValue !== 0;
            });

            PollService.getPoll($scope.pollId)
                .then(function (pollData) {
                    $scope.poll = pollData.data;

                    $scope.poll.Choices.forEach(function (opt) { opt.voteValue = 0; });

                    currentlySelectedChoices.forEach(function (selectedChoice) {
                        $scope.poll.Choices.forEach(function (choice) {
                            // Note that this is subtly different from the initial load (as we have Id vs Id here, and Id vs ChoiceId above)
                            if (choice.Id === selectedChoice.Id) {
                                choice.voteValue = selectedChoice.voteValue;
                            }
                        });
                    });

                });
        }

        function clearVote() {
            if (!$scope.token || $scope.token.length === 0) {
                // TODO: Inform the user that they somehow don't have a token
                return;
            }

            var voterName = (IdentityService.identity && $scope.poll && $scope.poll.NamedVoting) ? IdentityService.identity.name : null;

            var emptyBallot = {
                VoterName: voterName,
                Votes: []
            };

            VoteService.submitVote($scope.pollId, emptyBallot, $scope.token)
                .then(function () {
                    RoutingService.navigateToResultsPage($scope.pollId, $scope.token);
                });
        }

        function submitVote(choices) {
            if (!choices) {
                return null;
            }

            if (!$scope.token || $scope.token.length === 0) {
                // TODO: Inform the user that they somehow don't have a token
                return;
            }

            if (!IdentityService.identity && $scope.poll && $scope.poll.NamedVoting) {
                return IdentityService.openLoginDialog($scope, function () {
                    submitVote(choices);
                });
            }

            var voterName = (IdentityService.identity && $scope.poll && $scope.poll.NamedVoting) ? IdentityService.identity.name : null;
            var votes = getVotes(choices);

            var ballot = {
                VoterName: voterName,
                Votes: votes
            };

            VoteService.submitVote($scope.pollId, ballot, $scope.token)
                .then(function () {
                    RoutingService.navigateToResultsPage($scope.pollId, $scope.token);
                });
        }

        function redirectIfExpired() {
            window.location.replace($scope.resultsLink);
        }
    }
})();
