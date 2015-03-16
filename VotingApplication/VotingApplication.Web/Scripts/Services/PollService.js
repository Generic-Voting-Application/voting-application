/// <reference path="AccountService.js" />
(function () {
    angular
        .module('GVA.Poll')
        .factory('PollService', PollService);


    PollService.$inject = ['$http', 'AccountService'];

    function PollService($http, AccountService) {

        var service = {
            getPoll: getPoll,
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

        function createPoll(question, successCallback) {

            var request = {
                method: 'POST',
                url: 'api/poll',
                headers: {
                    'Content-Type': 'application/json; charset=utf-8',
                    'Authorization': 'Bearer ' + AccountService.account.token
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
            };

            $http(request)
                .success(successCallback);

        }
    }
})();
