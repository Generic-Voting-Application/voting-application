(function() {
    var VotingApp = angular.module('VotingApp');

    VotingApp.factory('pollData', ['$location', function ($location) {
        var self = this;

        self.currentPollId = function () {
            var urlParams = $location.url().split("/");

            if (urlParams.length < 3)
                return;

            var pollId = urlParams[2];

            return pollId;
        }

        return self;
    }]);
})();
