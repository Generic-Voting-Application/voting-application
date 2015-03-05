(function () {
    angular.module('GVA.Creation', ['ngRoute', 'ngDialog', 'ngStorage', 'GVA.Common', 'GVA.Voting']).config(['$routeProvider', function ($routeProvider) {
        // TODO: GVA.Voting should not be required, it should be GVA.Polls, but it's not been created yet.
    }]);
})();
