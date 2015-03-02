(function () {
    angular.module('GVA.Creation').controller('ManagePageController', ['$scope', '$routeParams', 'AccountService', 'PollService',
            function ($scope, $routeParams, AccountService, PollService) {

                var manageId = $routeParams.manageId;
                
                $scope.poll = {};
                $scope.manageId = manageId;

                $scope.formatPollExpiry = function(){
                    if(!$scope.poll.Expires || !$scope.poll.ExpiryDate){
                        return 'Never';
                    }

                    var expiryDate = new Date($scope.poll.ExpiryDate);
                    return expiryDate.toLocaleString();
                }
                
                PollService.getPollByManageId(manageId, function (data) {
                    $scope.poll = data;
                });

            }]);
})();
