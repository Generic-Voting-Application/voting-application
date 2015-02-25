(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.directive('createStrategy', ['AccountService', function (AccountService) {

        var createTemplate = function () {
            return '../Routes/createBasic';
        }

        return {
            replace: true,
            templateUrl: createTemplate()
        }
    }]);
})();