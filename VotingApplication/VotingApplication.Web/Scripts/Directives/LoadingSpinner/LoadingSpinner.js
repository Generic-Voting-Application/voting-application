(function () {
    'use strict';

    angular
        .module('VoteOn-Common')
        .directive('loadingSpinner', loadingSpinner);

    function loadingSpinner() {
        return {
            restrict: 'E',
            replace: true,
            templateUrl: '/Scripts/Directives/LoadingSpinner/LoadingSpinner.html',
            scope: {
                loaded: '='
            }
        };
    }
})();