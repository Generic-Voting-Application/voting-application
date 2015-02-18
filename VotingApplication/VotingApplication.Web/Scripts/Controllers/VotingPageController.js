(function () {
    var VotingApp = angular.module('VotingApp');
    VotingApp.controller('VotingPageController', function ($scope, $location) {
        console.log("Voting");

        // Turn "/#/voting/abc/123" into "/#/results/abc/123"
        var locationTokens = $location.url().split("/");
        locationTokens.splice(0, 2);
        $scope.resultsLink = '/#/results/' + locationTokens.join("/");
    });
})();
