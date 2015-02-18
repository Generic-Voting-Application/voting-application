(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.directive('votingStrategy', ['PollStrategy', function (PollStrategy) {
        return {
            replace: true,

            link: function (scope, element, attrs) {
                scope.votingTemplate = PollStrategy.votingTemplate;
            },

            template: '<div ng-include="votingTemplate()"></div>'
        }
    }]);
})();
