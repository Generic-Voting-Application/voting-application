(function () {
    'use strict';

    angular
        .module('VoteOn-Poll')
        .factory('IdentityService', IdentityService);


    IdentityService.$inject = ['$localStorage'];

    function IdentityService($localStorage) {

        if ($localStorage.identity === undefined) {
            $localStorage.identity = { name: '' };
        }

        var service = {
            identity: $localStorage.identity
        };

        return service;
    }
})();