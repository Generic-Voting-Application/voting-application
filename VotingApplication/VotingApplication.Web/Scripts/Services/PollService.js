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


        function getPoll(pollId) {

            if (!pollId) {
                var deferred = $q.defer();
                deferred.reject();
                return deferred.promise;
            }

            return $http({
                method: 'GET',
                url: '/api/poll/' + pollId
            });
        }

        function getUserPolls() {

            return $http({
                method: 'GET',
                url: '/api/dashboard/polls',
                headers: { 'Authorization': 'Bearer ' + AccountService.account.token }
            });
        }

        function createPoll(question, choices) {
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
                data: JSON.stringify({
                    PollName: question,
                    Choices: choices
                })
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
