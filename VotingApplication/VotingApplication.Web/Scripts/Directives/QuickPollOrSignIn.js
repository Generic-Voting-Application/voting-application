(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.directive('quickPollOrSignIn', function () {

        var pageTemplate = function () {
            if (!this.createPoll) {
                return 'routes/createHome';
            }

            return 'routes/create';
        }

        return {
            replace: true,

            link: function (scope, element, attrs) {
                scope.pageTemplate = pageTemplate;
            },

            template: '<div ng-include="pageTemplate()"></div>'
        }
    });
})();
