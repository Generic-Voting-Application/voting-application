(function () {
    'use strict';

    angular
        .module('GVA.Voting')
        .factory('VoteService', VoteService);


    VoteService.$inject = ['$location', '$http', '$q', 'Errors'];

    function VoteService($location, $http, $q, Errors) {

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

        function getResults(pollId, ballotToken) {

            if (!lastCheckedTimestamps[pollId]) {
                lastCheckedTimestamps[pollId] = 0;
            }

            var tokenGuidHeader = null;

            if (ballotToken) {
                tokenGuidHeader = { 'X-TokenGuid': ballotToken };
            }

            var prom = $q.defer();

            return $http({
                method: 'GET',
                url: '/api/poll/' + pollId + '/results?lastRefreshed=' + lastCheckedTimestamps[pollId],
                headers: tokenGuidHeader
            })
                .then(function (response) {
                    lastCheckedTimestamps[pollId] = Date.now();

                    prom.resolve(response.data);
                    return prom.promise;
                })
                .catch(function (response) {
                    return transformError(response, prom);
                });
        }

        function transformError(response, promise) {

            switch (response.status) {
                case 400:
                    {
                        promise.reject(Errors.IncorrectPollOrder);
                        break;
                    }
                case 401:
                    {
                        promise.reject(Errors.PollInviteOnlyNoToken);
                        break;
                    }
                case 403:
                    {
                        promise.reject(Errors.IncorrectPollOrder);
                        break;
                    }
                case 404:
                    {
                        promise.reject(Errors.PollNotFound);
                        break;
                    }
                case 304:
                    {
                        promise.resolve();
                        break;
                    }
                default:
                    promise.reject(response.readableMessage);
            }

            return promise.promise;
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
