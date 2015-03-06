(function () {
    angular
        .module('GVA.Creation')
        .controller('ManageOptionController', ['$scope', '$routeParams', '$location', 'ManageService',
        function ($scope, $routeParams, $location, ManageService) {

            $scope.poll = ManageService.poll;
            $scope.manageId = $routeParams.manageId;

            $scope.updatePoll = function () {
                ManageService.updatePoll($routeParams.manageId, $scope.poll, function () {
                    ManageService.getPoll($scope.manageId);
                });

            }

            $scope.return = function () {
                $location.path('Manage/' + $scope.manageId);
            }

            $scope.remove = function (option) {
                $scope.poll.Options.splice($scope.poll.Options.indexOf(option), 1);
                $scope.updatePoll();
            }

            $scope.clear = function (form) {
                form.Name = '';
                form.Description = '';
                form.$setPristine();
            }

            $scope.add = function (optionForm) {
                var newOption = {
                    Name: optionForm.Name,
                    Description: optionForm.Description
                };

                $scope.poll.Options.push(newOption);
                $scope.updatePoll();
            }

            ManageService.registerPollObserver(function () {
                $scope.poll = ManageService.poll;
            })
        }]);
})();
