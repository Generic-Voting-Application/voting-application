/// <reference path="AccountService.js" />
(function () {
    angular
        .module('GVA.Poll')
        .factory('PollService', PollService);


    PollService.$inject = ['$http', 'AccountService'];

    function PollService($http, AccountService) {

        var service = {
            getPoll: getPoll,
            getUserPolls: getUserPolls,
            createPoll: createPoll
        };

        return service;


        function getPoll(pollId, callback, failureCallback) {

            if (!pollId) {
                return null;
            }

            $http({
                method: 'GET',
                url: '/api/poll/' + pollId
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

        function getUserPolls() {


            var promise = $http({
                method: 'GET',
                url: '/api/dashboard/polls',
                headers: { 'Authorization': 'Bearer ' + AccountService.account.token }
            });

            return promise;

        }

        function createPoll(question, successCallback) {
            var token;
            if (AccountService.account) {
                token = AccountService.account.token;
            }
            else {
                token = null;
            }

            var request = {
                method: 'POST',
                url: 'api/poll',
                headers: {
                    'Content-Type': 'application/json; charset=utf-8',
                    'Authorization': 'Bearer ' + token
                },
                data: JSON.stringify({ PollName: question })
            };

            $http(request)
                .success(successCallback);

        }
    }
})();
