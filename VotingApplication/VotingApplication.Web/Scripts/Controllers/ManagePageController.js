(function () {
    angular.module('GVA.Creation').controller('ManagePageController', ['$scope', '$routeParams', 'AccountService', 'ManageService',
            function ($scope, $routeParams, AccountService, ManageService) {

                var manageId = $routeParams.manageId;
                
                $scope.poll = {};
                $scope.manageId = manageId;

                $scope.openLoginDialog = function () {
                    AccountService.openLoginDialog($scope);
                }

                $scope.formatPollExpiry = function(){
                    if(!$scope.poll.Expires || !$scope.poll.ExpiryDate){
                        return 'Never';
                    }

                    var expiryDate = new Date($scope.poll.ExpiryDate);
                    return expiryDate.toLocaleString();
                }
                
                ManageService.getPoll(manageId, function (data) {
                    $scope.poll = data;
                });

            }]);
})();
