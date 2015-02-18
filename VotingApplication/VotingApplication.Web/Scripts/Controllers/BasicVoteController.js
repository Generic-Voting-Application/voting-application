(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.controller('BasicVoteController', ['$scope', 'IdentityService', 'PollService', function ($scope, IdentityService, PollService) {

        var pollId = PollService.currentPollId();
        
        $scope.vote = function (options) {
            if (IdentityService.identityName) {
                // Do the voting stuff here
                console.log(options);
            } else {
                IdentityService.openLoginDialog($scope);
            }
        }

        PollService.getPoll(pollId, function (data) {
            $scope.options = data.Options;
        });

    }]);

})();
