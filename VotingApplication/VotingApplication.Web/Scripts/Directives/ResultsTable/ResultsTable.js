(function () {
    'use strict';

    angular
        .module('VoteOn-Components')
        .directive('resultsTable', resultsTable);

    function resultsTable() {
        
        var resultToggles = {};

        function link(scope) {

            scope.toggleResult = toggleResult;
            scope.isToggled = isToggled;
            
            function toggleResult(number) {
                resultToggles[number] = !resultToggles[number];
            }

            function isToggled(number) {
                return resultToggles[number];
            }

        }

        return {
            restrict: 'EA',
            link : link,
            scope: {
                resultBreakdown: '=',
            },
            templateUrl: '/Scripts/Directives/ResultsTable/ResultsTable.html'
        };
    }

})();
