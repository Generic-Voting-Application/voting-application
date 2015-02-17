(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.factory('PollStrategy', ['PollAction', function (PollAction) {
        var pollStrategy;

        PollAction.getPoll(PollAction.currentPollId(), function (data) {
            pollStrategy = data.VotingStrategy;
        });

        var getVotingTemplate = function () {
            if (!pollStrategy) {
                return 'routes/BasicVote';
            }

            return 'routes/' + pollStrategy + 'Vote';
        }

        return {
            hasVotingTemplate: function() { return pollStrategy != undefined; },
            getVotingTemplate: getVotingTemplate
        };
    }]);
})();