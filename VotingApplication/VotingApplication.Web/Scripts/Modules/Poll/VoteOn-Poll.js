(function () {
    'use strict';

    angular
        .module('VoteOn-Poll', ['VoteOn-Common', 'VoteOn-Account', 'VoteOn-Vote', 'VoteOn-Results'])
        .config([
        '$routeProvider',
        function ($routeProvider) {
            $routeProvider
                .when('/:pollId/Vote/:tokenId?', {
                    templateUrl: function () {
                        return '../Poll/Vote';
                    }
                })
                .when('/:pollId/Results', {
                    templateUrl: function () {
                        return '../Poll/Results';
                    }
                })
                .when('/:pollId/InviteOnly', {
                    templateUrl: function () {
                        return '../Poll/InviteOnly';
                    }
                });
        }]);
})();
