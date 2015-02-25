(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.directive('pollHeading', function () {

        return {
            replace: true,
            templateUrl: '../Routes/PollHeading'
        }
    });
})();
