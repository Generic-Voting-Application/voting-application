/// <reference path="../Services/PollService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Voting')
        .directive('votingStrategy', votingStrategy);

    votingStrategy.$inject = ['$routeParams', 'PollService'];

    function votingStrategy($routeParams, PollService) {

        var pollStrategy = null;
        var pollId = $routeParams.pollId;

        PollService.getPoll(pollId, function (data) {
            pollStrategy = data.VotingStrategy;
        });

        var votingTemplate = function () {
            if (!pollStrategy) {
                return '';
            }

            return '../Routes/' + pollStrategy + 'Vote';
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
