(function () {
    'use strict';

    angular
    .module('GVA.Common')
    .directive('errorBar', ErrorBar);

    function ErrorBar() {

        return {
            templateUrl: '/Routes/ErrorBar',
            restrict: 'A',
        };
    }
})();