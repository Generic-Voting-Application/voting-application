(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.controller('HomePageController', ['$scope', 'PollAction', function ($scope, PollAction) {
        $scope.models = {
            pageTitle: 'Pollster'
        };

        PollAction.getPoll(PollAction.currentPollId(), function (data) {
            $scope.pollName = data.Name
        })
    }]);

})();