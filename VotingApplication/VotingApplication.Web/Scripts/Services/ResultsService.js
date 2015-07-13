(function () {
    'use strict';

    angular
        .module('VoteOn-Results')
        .factory('ResultsService', ResultsService);


    ResultsService.$inject = ['$http', '$q'];

    function ResultsService($http, $q) {

        var lastCheckedTimestamps = {};

        var service = {
            getResults: getResults,
            createDataTable: createDataTable
        };

        return service;

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
                lastCheckedTimestamps[pollId] = moment().utc().valueOf();

                prom.resolve(response.data);
                return prom.promise;
            })
            .catch(function (response) {
                if (response.status === 304) {
                    prom.resolve(response.data);
                } else {
                    prom.reject(response);
                }
                return prom.promise;
            });
        }

        function createDataTable(results, trimSize) {
            if (!results) {
                return null;
            }

            var sortedResults = results.sort(function (a, b) {
                return a.Sum < b.Sum;
            });

            var table = [];

            table[0] = ['Choice', 'Votes'];

            sortedResults.forEach(function (result) {
                var trimmedChoiceName = result.ChoiceName.substring(0, Math.round(trimSize));
                if (trimmedChoiceName !== result.ChoiceName) {
                    trimmedChoiceName = trimmedChoiceName + '...';
                }

                table.push([trimmedChoiceName, result.Sum]);
            });

            return table;
        }
    }
})();
