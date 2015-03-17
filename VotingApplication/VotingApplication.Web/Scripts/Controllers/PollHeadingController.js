/// <reference path="../Services/PollService.js" />
/// <reference path="../Services/ExpiryStringService.js" />
(function () {
    angular
        .module('GVA.Voting')
        .controller('PollHeadingController', PollHeadingController);

    PollHeadingController.$inject = ['$scope', '$routeParams', 'PollService', 'ExpiryStringService'];

    function PollHeadingController($scope, $routeParams, PollService, ExpiryStringService) {
        $scope.pollExpiry = undefined;

        var pollId = $routeParams.pollId;

        var calculateExpiry = function (expiryDate) {
            $scope.pollExpiry = ExpiryStringService.timeStringForExpiry(expiryDate, calculateExpiry);
            $scope.$apply();
        };

        PollService.getPoll(pollId, function (data) {
            $scope.pollName = data.Name;

            if (data.ExpiryDate) {
                $scope.pollExpiry = ExpiryStringService.timeStringForExpiry(new Date(data.ExpiryDate), calculateExpiry);
            }
        });
    }
})();
