(function () {
    'use strict';

    angular
        .module('VoteOn-Components')
        .directive('dateTimePicker', dateTimePicker);

    function dateTimePicker() {
        return {
            restrict: 'E',
            replace: true,
            templateUrl: '/Scripts/Directives/DateTimePicker/DateTimePicker.html',
            scope: {

            }
        };
    }
})();
