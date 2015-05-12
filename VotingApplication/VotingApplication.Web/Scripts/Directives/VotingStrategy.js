/// <reference path="../Services/PollService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Voting')
        .directive('votingStrategy', votingStrategy);

    votingStrategy.$inject = ['$routeParams', 'PollService'];

    function votingStrategy($routeParams, PollService) {

        var pollType = null;
        var pollId = $routeParams.pollId;

        PollService.getPoll(pollId)
            .then(function (response) {
                pollType = response.data.PollType;
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
