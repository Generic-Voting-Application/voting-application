(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.directive('votingStrategy', ['PollStrategy', function (PollStrategy) {
        return {
            replace: true,

            link: function (scope, element, attrs) {
                scope.hasVotingTemplate = PollStrategy.hasVotingTemplate;
                scope.getVotingTemplate = PollStrategy.getVotingTemplate;
            },

            template: '<div ng-include="getVotingTemplate()" ng-show="hasVotingTemplate()"></div>'
        }
    }]);
})();
