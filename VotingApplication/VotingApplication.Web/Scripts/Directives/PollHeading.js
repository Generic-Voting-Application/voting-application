(function () {
    angular
        .module('GVA.Voting')
        .directive('pollHeading', pollHeading);

    function pollHeading() {
        return {
            replace: true,
            templateUrl: '/Scripts/Directives/PollHeading.html'
        }
    }
})();
