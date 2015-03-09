(function () {
    angular
        .module('GVA.Creation')
        .controller('ManageOptionController', ManageOptionController);

    ManageOptionController.$inject = ['$scope', '$routeParams', '$location', 'ManageService'];

    function ManageOptionController($scope, $routeParams, $location, ManageService) {

        $scope.poll = ManageService.poll;
        $scope.manageId = $routeParams.manageId;
        $scope.updatePoll = updatePollDetails;
        $scope.return = navigateToManagePage;
        $scope.remove = removePollOption;
        $scope.clear = clearPollOption;
        $scope.add = addPollOption;

        activate();


        function activate() {
            ManageService.registerPollObserver(function () {
                $scope.poll = ManageService.poll;
            });
        }

        function navigateToManagePage() {
            $location.path('Manage/' + $scope.manageId);
        };

        function removePollOption(option) {
            $scope.poll.Options.splice($scope.poll.Options.indexOf(option), 1);
            $scope.updatePoll();
        };

        function clearPollOption(form) {
            form.Name = '';
            form.Description = '';
            form.$setPristine();
        }

        function addPollOption(optionForm) {
            var newOption = {
                Name: optionForm.Name,
                Description: optionForm.Description
            };

            $scope.poll.Options.push(newOption);
            $scope.updatePoll();
        };

        function updatePollDetails() {
            ManageService.updatePoll($routeParams.manageId, $scope.poll, updatePollSuccessCallback);
        };

        function updatePollSuccessCallback() {
            ManageService.getPoll($scope.manageId);
        };
    }

})();
