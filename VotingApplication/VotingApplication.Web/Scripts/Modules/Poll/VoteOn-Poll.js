(function () {
    'use strict';

    angular
        .module('VoteOn-Poll', ['VoteOn-Common', 'VoteOn-Account', 'VoteOn-Vote'])
            .config([
            '$routeProvider',
            function ($routeProvider) {
                $routeProvider
                    .when('/:pollId/Vote', {
                        templateUrl: function () {
                            return '../Poll/Vote';
                        }
                    });
            }]);
})();
