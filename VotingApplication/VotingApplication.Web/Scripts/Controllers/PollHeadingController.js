(function () {
    angular.module('GVA.Voting').controller('PollHeadingController', ['$scope', 'PollService', 'ExpiryStringService', function ($scope, PollService, ExpiryStringService) {
        $scope.pollExpiry = undefined;

        var calculateExpiry = function (expiryDate) {
            $scope.pollExpiry = ExpiryStringService.timeStringForExpiry(expiryDate, calculateExpiry);
            $scope.$apply();
        }

        PollService.getPoll(PollService.currentPollId(), function (data) {
            $scope.pollName = data.Name
            if (data.Expires) {
                $scope.pollExpiry = ExpiryStringService.timeStringForExpiry(new Date(data.ExpiryDate), calculateExpiry);
            }
        });
    }]);
})();
