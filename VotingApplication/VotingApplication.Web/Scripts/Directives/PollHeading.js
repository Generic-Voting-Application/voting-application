/// <reference path="../Services/PollService.js" />
/// <reference path="../Services/ExpiryStringService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Voting')
        .directive('pollHeading', pollHeading);

    pollHeading.$inject = ['$routeParams', 'PollService', 'ExpiryStringService'];

    function pollHeading($routeParams, PollService, ExpiryStringService) {

        function link($scope) {
            var pollId = $routeParams.pollId;

            function calculateExpiry(expiryDateUtc) {
                $scope.pollExpiry = ExpiryStringService.timeStringForExpiry(expiryDateUtc, calculateExpiry);
                $scope.$apply();

                resolveExpiryCallback(expiryDateUtc);
            }

            function resolveExpiryCallback(expiryDateUtc) {
                if (expiryDateUtc < moment.utc()) {
                    $scope.gvaExpiredCallback();
                }
            }

            PollService.getPoll(pollId)
                .then(function (response) {
                    var data = response.data;

                    if (data.ExpiryDateUtc) {
                        var expiryDateUtc = moment.utc(data.ExpiryDateUtc);
                        $scope.pollExpiry = ExpiryStringService.timeStringForExpiry(expiryDateUtc, calculateExpiry);
                        resolveExpiryCallback(expiryDateUtc);
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
