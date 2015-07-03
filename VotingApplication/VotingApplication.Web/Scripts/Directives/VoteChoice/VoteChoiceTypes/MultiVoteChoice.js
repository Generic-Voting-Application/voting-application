(function () {
    'use strict';

    angular
        .module('VoteOn-Vote')
        .directive('multiVoteChoice', multiVoteChoice);

    function multiVoteChoice() {
        return {
            restrict: 'E',
            scope: {
                choices: '='
            },
            templateUrl: '/Scripts/Directives/VoteChoice/VoteChoiceTypes/MultiVoteChoice.html'
        };
    }
})();