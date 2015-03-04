(function () {
    angular.module('GVA.Creation').controller('ManageOptionController', ['$scope', '$routeParams', '$location', 'ManageService', function ($scope, $routeParams, $location, ManageService) {

        $scope.poll = ManageService.poll;
        $scope.manageId = $routeParams.manageId;

        var updatePoll = function () {
            ManageService.updatePoll($routeParams.manageId, $scope.poll, function () {
                ManageService.getPoll($scope.manageId);
            });

        }

        $scope.return = function () {
            $location.path('Manage/' + $scope.manageId);
        }

        $scope.remove = function (option) {
            $scope.poll.Options.splice($scope.poll.Options.indexOf(option), 1);
            updatePoll();
        }

        ManageService.registerPollObserver(function () {
            $scope.poll = ManageService.poll;
        })
    }]);
})();
