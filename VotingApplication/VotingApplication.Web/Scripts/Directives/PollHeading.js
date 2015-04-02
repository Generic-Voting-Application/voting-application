(function () {
    "use strict";

    angular
        .module('GVA.Voting')
        .directive('pollHeading', pollHeading);

    pollHeading.$inject = ['$routeParams', 'PollService', 'ExpiryStringService'];

    function pollHeading($routeParams, PollService, ExpiryStringService) {

        function link($scope) {
            var pollId = $routeParams.pollId;

            function calculateExpiry(expiryDate) {
                $scope.pollExpiry = ExpiryStringService.timeStringForExpiry(expiryDate, calculateExpiry);
                $scope.$apply();

                resolveExpiryCallback(expiryDate);
            }

            function resolveExpiryCallback(expiryDate) {
                if (expiryDate < Date.now()) {
                    $scope.gvaExpiredCallback();
                }
            }

            PollService.getPoll(pollId, function (data) {
                $scope.pollName = data.Name;

                if (data.ExpiryDate) {
                    var expiryDate = new Date(data.ExpiryDate);
                    $scope.pollExpiry = ExpiryStringService.timeStringForExpiry(expiryDate, calculateExpiry);
                    resolveExpiryCallback(expiryDate);
                }
            });
        }

        return {
            restrict: 'E',
            replace: true,
            templateUrl: '/Scripts/Directives/PollHeading.html',
            link: link,
            scope: {
                gvaExpiredCallback: '&'
            }
        };
    }
})();
