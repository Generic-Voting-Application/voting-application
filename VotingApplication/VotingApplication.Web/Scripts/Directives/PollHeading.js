(function () {
    angular
        .module('GVA.Voting')
        .directive('pollHeading', pollHeading);

    function pollHeading() {
        return {
            replace: true,
            templateUrl: '../Routes/PollHeading'
        }
    }
})();
