(function () {
    angular.module('GVA.Creation', ['GVA.Common']).directive('createStrategy', ['AccountService', function (AccountService) {

        var createTemplate = function () {
            return '../Routes/createBasic';
        }

        return {
            replace: true,
            templateUrl: createTemplate()
        }
    }]);
})();