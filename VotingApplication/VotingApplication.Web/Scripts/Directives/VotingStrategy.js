/// <reference path="../Services/PollService.js" />
/// <reference path="../Services/TokenService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Voting')
        .directive('votingStrategy', votingStrategy);

    votingStrategy.$inject = ['$routeParams', 'PollService', 'TokenService'];

    function votingStrategy($routeParams, PollService, TokenService) {

        var pollType = null;
        var pollId = $routeParams.pollId;

        var token = TokenService.retrieveToken(pollId);

        PollService.getPoll(pollId, token)
            .then(function (data) {
                pollType = data.PollType;
            });

        var votingTemplate = function () {
            if (!pollType) {
                return '';
            }

            return '../Routes/' + pollType + 'Vote';
        };

        return {
            replace: true,
            link: function (scope) {
                scope.votingTemplate = votingTemplate;
            },

            template: '<div ng-include="votingTemplate()"></div>'
        };
    }
})();
