(function () {
    angular.module('GVA.Voting').factory('PollStrategy', ['PollService', function (PollService) {

        var self = this;

        var pollStrategy;

        PollService.getPoll(PollService.currentPollId(), function (data) {
            pollStrategy = data.VotingStrategy;
        });

        self.votingTemplate = function () {
            if (!pollStrategy) {
                return '';
            }

            return 'routes/' + pollStrategy + 'Vote';
        }

        return self;
    }]);
})();