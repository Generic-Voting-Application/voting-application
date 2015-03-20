(function() {
    angular
    .module('GVA.Common')
    .directive('gvaUnlockableOption', ['AccountService', UnlockableOption]);

    function UnlockableOption(AccountService) {

        function link(scope, element, attrs) {
            scope.openLoginDialog = function() {
                AccountService.openLoginDialog(scope);
            };

            scope.isLoggedIn = function () {
                return (scope.$parent.account !== undefined);
            };
        }

        return {
            templateUrl: '/Scripts/Directives/UnlockableOption.html',
            restrict: 'E',
            link: link,
            scope: {
                optionPath: '@'
            }
        };
    }
})();