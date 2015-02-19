(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.controller('HomePageController', ['$scope', 'PollService', 'ExpiryStringService', function ($scope, PollService, ExpiryStringService) {
        $scope.models = {
            pageTitle: 'Pollster'
        };

        $scope.pollExpiry = undefined;

        var calculateExpiry = function (expiryDate) {
            $scope.pollExpiry = ExpiryStringService.timeStringForExpiry(expiryDate, calculateExpiry);
            $scope.$apply();
        }

        PollService.getPoll(PollService.currentPollId(), function (data) {
            $scope.pollName = data.Name
            if (data.Expires) {
                calculateExpiry(new Date(data.ExpiryDate));
            }
        });
    }]);

})();