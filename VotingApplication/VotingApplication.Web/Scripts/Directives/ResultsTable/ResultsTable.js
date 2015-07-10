(function () {
    'use strict';

    angular
        .module('VoteOn-Components')
        .directive('resultsTable', resultsTable);

    function resultsTable() {
        
        function link() {
           
        }

        return {
            restrict: 'EA',
            scope: {
                resultBreakdown: '=',
            },
            link: link,
            templateUrl: '/Scripts/Directives/ResultsTable/ResultsTable.html'
        };
    }

})();
