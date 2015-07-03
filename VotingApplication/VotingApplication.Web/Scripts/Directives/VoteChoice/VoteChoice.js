
(function () {
    'use strict';

    angular
        .module('VoteOn-Vote')
        .directive('voteChoice', voteChoice);

    function voteChoice() {
        return {
            restrict: 'E',
            scope: {
                choices: '=',
                poll: '='
            },
            templateUrl: '/Scripts/Directives/VoteChoice/VoteChoice.html'
        };
    }
})();