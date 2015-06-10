/// <reference path="AccountService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Poll')
        .factory('PollService', PollService);


    PollService.$inject = ['$http', '$q', 'AccountService'];

    function PollService($http, $q, AccountService) {

        var service = {
            getPoll: getPoll,
            getUserPolls: getUserPolls,
            createPoll: createPoll,
            copyPoll: copyPoll
        };

        return service;


        function getPoll(pollId, ballotToken) {

            if (!pollId) {
                var deferred = $q.defer();
                deferred.reject();
                return deferred.promise;
            }

            var tokenGuidHeader = null;

            if (ballotToken) {
                tokenGuidHeader = { 'X-TokenGuid': ballotToken };
            }

            return $http({
                method: 'GET',
                url: '/api/poll/' + pollId,
                headers: tokenGuidHeader
            });
        }

        function getUserPolls() {

            return $http({
                method: 'GET',
                url: '/api/dashboard/polls',
                headers: { 'Authorization': 'Bearer ' + AccountService.account.token }
            });
        }

        function createPoll(question) {
            var token;
            if (AccountService.account) {
                token = AccountService.account.token;
            }
            else {
                token = null;
            }

            return $http({
                method: 'POST',
                url: 'api/poll',
                headers: {
                    'Content-Type': 'application/json; charset=utf-8',
                    'Authorization': 'Bearer ' + token
                },
                data: JSON.stringify({ PollName: question })
            });

        }

        function copyPoll(pollId) {
            return $http({
                method: 'POST',
                url: '/api/dashboard/copy',
                headers: {
                    'Content-Type': 'application/json; charset=utf-8',
                    'Authorization': 'Bearer ' + AccountService.account.token
                },
                data: JSON.stringify({ UUIDToCopy: pollId })
            });
        }
    }
})();
