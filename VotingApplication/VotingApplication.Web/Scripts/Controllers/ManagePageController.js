(function () {
    angular.module('GVA.Creation').controller('ManagePageController', ['$scope', '$routeParams', 'AccountService', 'PollService',
            function ($scope, $routeParams, AccountService, PollService) {

                var manageId = $routeParams.manageId;

                $scope.manageId = manageId;

                PollService.getPollByManageId(manageId, function (data) {
                    $scope.UUID = data.UUID;
                });

            }]);
})();
