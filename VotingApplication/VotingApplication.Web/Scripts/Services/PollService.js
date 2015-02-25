(function () {
    angular.module('GVA.Voting').factory('PollService', ['$location', '$http', function ($location, $http) {
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

            $http({
                method: 'PUT',
                url: '/api/token/' + token + '/poll/' + pollId + '/vote',
                data: votes
            })
            .success(function (data, status) { if (callback) { callback(data, status) } })
            .error(function (data, status) { if (callback) { callback(data, status) } });
        }

        self.getPoll = function (pollId, callback) {

            if (!pollId) {
                return null;
            }

            $http({
                method: 'GET',
                url: '/api/poll/' + pollId
            })
            .success(function (data, status) { if (callback) { callback(data, status) } })
            .error(function (data, status) { if (callback) { callback(data, status) } });

        }

        self.getResults = function (pollId, callback) {

            if (!pollId) {
                return null;
            }

            $http({
                method: 'GET',
                url: '/api/poll/' + pollId + '/vote'
            })
            .success(function (data, status) { if (callback) { callback(data, status) } })
            .error(function (data, status) { if (callback) { callback(data, status) } });

        }

        self.getTokenVotes = function (pollId, token, callback) {

            if (!pollId || !token) {
                return null;
            }

            $http({
                method: 'GET',
                url: '/api/token/' + token + '/poll/' + pollId + '/vote'
            })
            .success(function (data, status) { if (callback) { callback(data, status) } })
            .error(function (data, status) { if (callback) { callback(data, status) } });

        }

        return self;
    }]);
})();
