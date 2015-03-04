(function () {
    angular.module('GVA.Creation').controller('ManagePageController', ['$scope', '$routeParams', 'AccountService', 'ManageService',
            function ($scope, $routeParams, AccountService, ManageService) {

                var manageId = $routeParams.manageId;
                var loaded = false;

                $scope.poll = {};
                $scope.manageId = manageId;

                $scope.openLoginDialog = function () {
                    AccountService.openLoginDialog($scope);
                }

                $scope.update = function () {
                    if (loaded) {
                        ManageService.poll = $scope.poll;
                        ManageService.updatePoll($routeParams.manageId, $scope.poll);
                    }
                };

                $scope.formatPollExpiry = function () {
                    if (!$scope.poll.Expires || !$scope.poll.ExpiryDate) {
                        return 'Never';
                    }

                    var expiryDate = new Date($scope.poll.ExpiryDate);
                    return expiryDate.toLocaleString();
                }

                ManageService.getPoll(manageId, function (data) {
                    $scope.poll = data;
                    loaded = true;
                });

            }]);
})();
