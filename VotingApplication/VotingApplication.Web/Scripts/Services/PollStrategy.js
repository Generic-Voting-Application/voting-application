(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.factory('PollStrategy', ['PollService', function (PollService) {
        var pollStrategy;

        PollService.getPoll(PollService.currentPollId(), function (data) {
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