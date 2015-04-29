(function () {
    'use strict';

    angular
        .module('GVA.Voting')
        .factory('VoteService', VoteService);


    VoteService.$inject = ['$location', '$http', '$q'];

    function VoteService($location, $http, $q) {

        var lastCheckedTimestamps = {};

        var service = {
            submitVote: submitVote,
            getResults: getResults,
            getTokenVotes: getTokenVotes,
            refreshLastChecked: refreshLastChecked,
            addVoterOption: addVoterOption
        };

        return service;

        function submitVote(pollId, votes, token) {

            if (!pollId || !votes || !token) {
                return null;
            }

            return $http({
                method: 'PUT',
                url: '/api/poll/' + pollId + '/token/' + token + '/vote',
                data: votes
            });

        }

        function getResults(pollId) {

            if (!lastCheckedTimestamps[pollId]) {
                lastCheckedTimestamps[pollId] = 0;
            }

            return $http({
                method: 'GET',
                url: '/api/poll/' + pollId + '/results?lastRefreshed=' + lastCheckedTimestamps[pollId]
            })
            .success(function (data, status) {

                lastCheckedTimestamps[pollId] = Date.now();
                return data, status;
            });
        }

        function getTokenVotes(pollId, token, callback, failureCallback) {

            if (!pollId || !token) {
                return null;
            }

            $http({
                method: 'GET',
                url: '/api/poll/' + pollId + '/token/' + token + '/vote'
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

        function addVoterOption(pollId, newOption) {
            var deferred = $q.defer();

            $http
                .post(
                    '/api/poll/' + pollId + '/option',
                    newOption
                )
                .success(function (data) { deferred.resolve(data); });

            return deferred.promise;
        }
    }
})();
