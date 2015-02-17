(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.controller('BasicVoteController', ['$scope', 'PollAction', function ($scope, PollAction) {

        var pollId = PollAction.currentPollId();

        PollAction.getPoll(pollId, function (data) {
            $scope.options = data.Options;
        });

    }]);

})();
