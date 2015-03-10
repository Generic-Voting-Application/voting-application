(function () {
    angular
        .module('GVA.Common')
        .factory('PollService', ['$location', '$http',
        function ($location, $http) {
            var self = this;

            var lastCheckedTimestamps = {};

            self.submitVote = function (pollId, votes, token, callback, failureCallback) {

                if (!pollId || !votes || !token) {
                    return null;
                }

                $http({
                    method: 'PUT',
                    url: '/api/token/' + token + '/poll/' + pollId + '/vote',
                    data: votes
                })
                .success(function (data) { if (callback) { callback(data) } })
                .error(function (data, status) { if (failureCallback) { failureCallback(data, status) } });

            }

            self.getPoll = function (pollId, callback, failureCallback) {

                if (!pollId) {
                    return null;
                }

                $http({
                    method: 'GET',
                    url: '/api/poll/' + pollId
                })
                .success(function (data) { if (callback) { callback(data) } })
                .error(function (data, status) { if (failureCallback) { failureCallback(data, status) } });

            }

            self.getResults = function (pollId, callback, failureCallback) {

                if (!pollId) {
                    return null;
                }

                if (!lastCheckedTimestamps[pollId]) {
                    lastCheckedTimestamps[pollId] = 0;
                }

                $http({
                    method: 'GET',
                    url: '/api/poll/' + pollId + '/vote?lastPoll=' + lastCheckedTimestamps[pollId]
                })
                .success(function (data) { if (callback) { callback(data) } })
                .error(function (data, status) { if (failureCallback) { failureCallback(data, status) } });

                lastCheckedTimestamps[pollId] = Date.now();

            }

            self.getTokenVotes = function (pollId, token, callback, failureCallback) {

                if (!pollId || !token) {
                    return null;
                }

                $http({
                    method: 'GET',
                    url: '/api/token/' + token + '/poll/' + pollId + '/vote'
                })
                .success(function (data) { if (callback) { callback(data) } })
                .error(function (data, status) { if (failureCallback) { failureCallback(data, status) } });

            }

        self.createPoll = function (question, successCallback) {
            var request = {
                method: 'POST',
                url: 'api/poll',
                headers: {
                    'Content-Type': 'application/json; charset=utf-8'
                },
                data: JSON.stringify({
                    Name: question,
                    Creator: 'Anonymous',
                    Email: undefined,
                    TemplateId: 0,
                    VotingStrategy: 'Basic',
                    MaxPoints: 7,
                    MaxPerVote: 3,
                    InviteOnly: false,
                    NamedVoting: false,
                    RequireAuth: false,
                    Expires: false,
                    ExpiryDate: undefined,
                    OptionAdding: false
                })
            }

            $http(request)
                .success(successCallback);

        };

            return self;
        }]);
})();
