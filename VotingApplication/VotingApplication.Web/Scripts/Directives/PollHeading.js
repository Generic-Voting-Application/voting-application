(function () {
    angular
        .module('GVA.Voting')
        .directive('pollHeading',
        function () {
            return {
                replace: true,
                templateUrl: '../Routes/PollHeading'
            }
        });
})();
