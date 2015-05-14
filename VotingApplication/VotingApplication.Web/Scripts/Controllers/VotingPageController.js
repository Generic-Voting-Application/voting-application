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

        $scope.poll = { Options: [] };
        $scope.resultsLink = RoutingService.getResultsPageUrl($scope.pollId, $scope.token);

        $scope.identityName = IdentityService.identity ? IdentityService.identity.name : null;
        $scope.logoutIdentity = IdentityService.clearIdentityName;
        $scope.gvaExpiredCallback = redirectIfExpired;
        $scope.submitVote = submitVote;
        $scope.clearVote = clearVote;

        var getVotes = function () { return []; };
        $scope.setVoteCallback = function (votesFunc) { getVotes = votesFunc; };

        activate();

        function activate() {

            $scope.$on('voterOptionAddedEvent', optionAdded);

            // Angular won't auto update this so we need to use the observer pattern
            IdentityService.registerIdentityObserver(function () {
                $scope.identityName = IdentityService.identity ? IdentityService.identity.name : null;
            });

            getToken();
        }

        function getToken() {
            TokenService.getToken($scope.pollId)
            .then(function (tokenData) { $scope.token = tokenData; })
            .then(getPollData);
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

                $scope.poll.Options.forEach(function (opt) { opt.voteValue = 0; });

                if (voteData) {
                    voteData.forEach(function (vote) {
                        $scope.poll.Options.forEach(function (option) {
                            if (option.Id === vote.OptionId) {
                                option.voteValue = vote.VoteValue;
                            }
                        });
                    });
                }
            });
        }

        function optionAdded() {

            var currentlySelectedOptions = $scope.poll.Options.filter(function (opt) {
                return opt.voteValue !== 0;
            });

            PollService.getPoll($scope.pollId)
                .then(function (pollData) {
                    $scope.poll = pollData.data;

                    $scope.poll.Options.forEach(function (opt) { opt.voteValue = 0; });

                    currentlySelectedOptions.forEach(function (selectedOption) {
                        $scope.poll.Options.forEach(function (option) {
                            // Note that this is subtly different from the initial load (as we have Id vs Id here, and Id vs OptionId above)
                            if (option.Id === selectedOption.Id) {
                                option.voteValue = selectedOption.voteValue;
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

            VoteService.submitVote($scope.pollId, [], $scope.token)
                .then(function () {
                    RoutingService.navigateToResultsPage($scope.pollId, $scope.token);
                });
        }

        function submitVote(options) {
            if (!options) {
                return null;
            }

            if (!$scope.token || $scope.token.length === 0) {
                // TODO: Inform the user that they somehow don't have a token
                return;
            }

            if (!IdentityService.identity && $scope.poll && $scope.poll.NamedVoting) {
                return IdentityService.openLoginDialog($scope, function () {
                    submitVote(options);
                });
            }

            var voterName = (IdentityService.identity && $scope.poll && $scope.poll.NamedVoting) ? IdentityService.identity.name : null;
            var votes = getVotes(options);

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
