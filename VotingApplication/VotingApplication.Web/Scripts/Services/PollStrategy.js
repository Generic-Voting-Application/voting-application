(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.factory('PollStrategy', ['PollAction', function (PollAction) {
        var pollStrategy;

        PollAction.getPoll(PollAction.currentPollId(), function (data) {
            pollStrategy = data.VotingStrategy;
        });

        var votingTemplate = function () {
            if (!pollStrategy) {
                return '';
            }

            return 'routes/' + pollStrategy + 'Vote';
        }

        return {
            votingTemplate: votingTemplate
        };
    }]);
})();