(function () {
    'use strict';

    angular
        .module('VoteOn-Components')
        .directive('resultsTable', resultsTable);

    function resultsTable() {

        return {
            restrict: 'EA',
            scope: {
                resultBreakdown: '=',
            },
            templateUrl: '/Scripts/Directives/ResultsTable/ResultsTable.html'
        };
    }

})();
