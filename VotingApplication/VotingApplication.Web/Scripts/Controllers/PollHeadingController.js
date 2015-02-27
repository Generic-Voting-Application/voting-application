(function () {
    angular.module('GVA.Voting').controller('PollHeadingController', ['$scope', '$routeParams', 'PollService', 'ExpiryStringService',
        function ($scope, $routeParams, PollService, ExpiryStringService) {
            $scope.pollExpiry = undefined;

            var pollId = $routeParams.pollId;

            var calculateExpiry = function (expiryDate) {
                $scope.pollExpiry = ExpiryStringService.timeStringForExpiry(expiryDate, calculateExpiry);
                $scope.$apply();
            }

            PollService.getPoll(pollId, function (data) {
                $scope.pollName = data.Name
                if (data.Expires) {
                    $scope.pollExpiry = ExpiryStringService.timeStringForExpiry(new Date(data.ExpiryDate), calculateExpiry);
                }
            });
        }]);
})();
