(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.controller('BasicVoteController', ['$scope', 'ngDialog', 'PollAction', function ($scope, ngDialog, PollAction) {

        var pollId = PollAction.currentPollId();

        PollAction.getPoll(pollId, function (data) {
            $scope.options = data.Options;
        });

        $scope.openLoginDialog = function(){
            ngDialog.open({
                template: 'Routes/LoginDialog',
                controller: 'LoginController'
            });
        }

    }]);

})();
