(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.factory('PollService', ['$location', '$http', function ($location, $http) {
        var self = this;

        self.currentPollId = function () {
            var urlParams = $location.url().split("/");

            if (urlParams.length < 3)
                return;

            var pollId = urlParams[2];

            return pollId;
        }

        self.submitVote = function (pollId, votes, token, callback) {

            if (!pollId || !votes || !token) {
                return null;
            }

            var postUri = '/api/token/' + token + '/poll/' + pollId + '/vote';

            $http.put(postUri, votes)
                .success(function (data, status) { if (callback) { callback(data, status) } })
                .error(function (data, status) { if (callback) { callback(data, status) } });
        }

        self.getPoll = function (pollId, callback) {

            if (!pollId) {
                return null;
            }

            var getUri = '/api/poll/' + pollId;

            $http.get(getUri)
                .success(function (data, status) { if (callback) { callback(data, status) } })
                .error(function (data, status) { if (callback) { callback(data, status) } });

        }

        self.getTokenVotes = function (pollId, token, callback) {

            if (!pollId || !token) {
                return null;
            }

            var getUri = '/api/token/' + tokenGuid + '/poll/' + pollId + '/vote';

            $http.get(getUri)
                .success(function (data, status) { if (callback) { callback(data, status) } })
                .error(function (data, status) { if (callback) { callback(data, status) } });

        }

        return self;
    }]);
})();
