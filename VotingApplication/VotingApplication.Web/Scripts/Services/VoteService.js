(function () {
    angular
        .module('GVA.Voting')
        .factory('VoteService', VoteService);


    VoteService.$inject = ['$location', '$http'];

    function VoteService($location, $http) {

        var lastCheckedTimestamps = {};

        var service = {
            submitVote: submitVote,
            getResults: getResults,
            getTokenVotes: getTokenVotes,
            refreshLastChecked: refreshLastChecked
        };

        return service;

        function submitVote(pollId, votes, token, callback, failureCallback) {

            if (!pollId || !votes || !token) {
                return null;
            }

            $http({
                method: 'PUT',
                url: '/api/token/' + token + '/poll/' + pollId + '/vote',
                data: votes
            })
            .success(function (data) {
                if (callback) {
                    callback(data);
                }
            })
            .error(
            function (data, status) {
                if (failureCallback) {
                    failureCallback(data, status);
                }
            });

        }

        function getResults(pollId, callback, failureCallback) {

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
            .success(function (data, status) {
                if (callback) {
                    callback(data, status);
                }
            })
            .error(function (data, status) {
                if (failureCallback) {
                    failureCallback(data, status);
                }
            });

            lastCheckedTimestamps[pollId] = Date.now();

        }

        function getTokenVotes(pollId, token, callback, failureCallback) {

            if (!pollId || !token) {
                return null;
            }

            $http({
                method: 'GET',
                url: '/api/token/' + token + '/poll/' + pollId + '/vote'
            })
            .success(function (data) {
                if (callback) {
                    callback(data);
                }
            })
            .error(function (data, status) {
                if (failureCallback) {
                    failureCallback(data, status);
                }
            });
        }

        function refreshLastChecked(pollId) {
            lastCheckedTimestamps[pollId] = 0;
        }
    }
})();
