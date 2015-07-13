(function () {
    'use strict';

    angular
        .module('VoteOn-Create')
        .factory('CreateService', CreateService);

    CreateService.$inject = ['$http', '$q', 'AccountService'];

    function CreateService($http, $q, AccountService) {
        
        var service = {
            createPoll: createPoll
        };

        return service;

        function createPoll(poll) {
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
                data: JSON.stringify(poll)
            })
            .then(function (response) {
                return response.data;
            });
        }
    }
})();