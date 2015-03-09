(function () {
    angular
        .module('GVA.Creation')
        .controller('ManagePageController', ['$scope', '$routeParams', 'AccountService', 'ManageService',
        function ($scope, $routeParams, AccountService, ManageService) {

            var manageId = $routeParams.manageId;

            $scope.poll = {};
            $scope.manageId = manageId;

            $scope.openLoginDialog = function () {
                AccountService.openLoginDialog($scope);
            };

            $scope.updatePoll = function () {
                ManageService.poll = $scope.poll;
                ManageService.updatePoll($routeParams.manageId, $scope.poll);
            };

            $scope.formatPollExpiry = function () {
                if (!$scope.poll.Expires || !$scope.poll.ExpiryDate) {
                    return 'Never';
                }

                var expiryDate = new Date($scope.poll.ExpiryDate);
                return expiryDate.toLocaleString();
            };

            $scope.formatVoters = function () {

                if ($scope.poll.InviteOnly) {
                    if ($scope.poll.AnonymousVoting) {
                        return 'Elections';
                    } else {
                        return 'Friends & Coworkers';
                    }
                } else {
                    if ($scope.poll.AnonymousVoting) {
                        return 'Survey Group';
                    } else {
                        return 'Social Media';
                    }
                }
            }

            $scope.selectText = function ($event) {
                $event.target.select();
            };

            ManageService.getPoll(manageId, function (data) {
                $scope.poll = data;
            });

        }]);
})();
