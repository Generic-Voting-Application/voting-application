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
                lastCheckedTimestamps[pollId] = moment().utc().toDate();

                prom.resolve(response.data);
                return prom.promise;
            })
            .catch(function () {
                //return transformError(response, prom);
            });
        }

        function createDataTable(results) {
            if (!results) {
                return null;
            }

            results.sort(function (a, b) {
                return a.Sum < b.Sum;
            });

            var table = [];

            table[0] = ['Choice', 'Votes'];

            results.forEach(function (result) {
                table.push([result.ChoiceName, result.Sum]);
            });

            return table;
        }
    }
})();
