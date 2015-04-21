(function () {
    'use strict';

    angular
    .module('GVA.Voting')
    .directive('pollInfoBox', PollInfoBox);

    function PollInfoBox() {

        return {
            templateUrl: '/Routes/PollInfoBox',
            restrict: 'A',
            scope: {
                poll: '=',
            }
        };
    }
})();