/// <reference path="../Services/RoutingService.js" />
/// <reference path="../Services/PollService.js" />
/// <reference path="../Services/TokenService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Manage')
        .controller('CreatePageController', CreatePageController);

    CreatePageController.$inject = ['$scope', 'RoutingService', 'TokenService', 'PollService'];

    function CreatePageController($scope, RoutingService, TokenService, PollService) {
        
        $scope.createQuickPoll = createQuickPoll;
        $scope.createCustomPoll = createCustomPoll;
        $scope.choiceIsPresent = choiceIsPresent;
        $scope.autoAddEmptyChoice = autoAddEmptyChoice;
        
        function createQuickPoll(question, choices) {
            var validChoices = [];

            if (choices && choices.length > 0) {
                choices.forEach(function (choice) {
                    if (choice.Name) {
                        validChoices.push({ Name: choice.Name });
                    }
                });
            }

            PollService.createPoll(question, validChoices)
            .then(createQuickPollSuccess);
        }

        function createCustomPoll(question) {
            PollService.createPoll(question)
            .then(createCustomPollSuccess);
        }

        function createQuickPollSuccess(response) {
            var data = response.data;

            TokenService.setManageId(data.UUID, data.ManageId)
            .then(function () {
                RoutingService.navigateToVotePage(data.UUID);
            });
        }

        function createCustomPollSuccess(response) {
            var data = response.data;

            TokenService.setManageId(data.UUID, data.ManageId)
                .then(TokenService.setToken(data.UUID, data.CreatorBallot.TokenGuid)
                .then(function () {
                    RoutingService.navigateToManagePage(data.ManageId);
                }));
        }

        function choiceIsPresent(choices) {
            if (choices) {
                for (var i = 0; i < choices.length; i++) {
                    if (choices[i].Name) {
                        return true;
                    }
                }
            }
            return false;
        }

        function autoAddEmptyChoice(choices) {
            if (choices && choices.length) {
                if (choices[choices.length - 1].Name) {
                    choices.push({});
                }
            }
        }
    }

})();
