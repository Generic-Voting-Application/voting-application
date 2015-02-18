(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.controller('BasicVoteController', ['$scope', 'ngDialog', 'IdentityService', 'PollService', function ($scope, ngDialog, IdentityService, PollService) {

        var pollId = PollService.currentPollId();

        var openLoginDialog = function (callback) {
            ngDialog.open({
                template: 'Routes/LoginDialog',
                controller: 'LoginController',
                scope: $scope,
                // The preclose callback must return true or the dialog will not close
                preCloseCallback: (callback ? function () { callback(); return true; } : undefined)
            });
        }

        $scope.vote = function (option) {
            if (IdentityService.identityName) {
                // Do the voting stuff here
                console.log(option);
            } else {
                openLoginDialog();
            }
        }

        PollService.getPoll(pollId, function (data) {
            $scope.options = data.Options;
        });

    }]);

})();
