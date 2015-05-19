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
            addVoterChoice: addVoterChoice
        };

        return service;

        function submitVote(pollId, votes, token) {
            if (!pollId || !votes || !token) {
                var deferred = $q.defer();
                deferred.reject();
                return deferred.promise;
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

        function getTokenVotes(pollId, token) {
            if (!pollId || !token) {
                var deferred = $q.defer();
                deferred.reject();
                return deferred.promise;
            }

            return $http({
                method: 'GET',
                url: '/api/poll/' + pollId + '/token/' + token + '/vote'
            });
        }

        function refreshLastChecked(pollId) {
            lastCheckedTimestamps[pollId] = 0;
        }

        function addVoterChoice(pollId, newChoice) {

            return $http
                .post(
                    '/api/poll/' + pollId + '/choice',
                    newChoice
                );
        }
    }
})();
