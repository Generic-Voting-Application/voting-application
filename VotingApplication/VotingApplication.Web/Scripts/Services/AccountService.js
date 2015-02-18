(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.factory('AccountService', ['$rootScope', '$location', '$http', '$localStorage', 'TokenService', function ($rootScope, $location, $http, $localStorage, TokenService) {

        var self = this;

        var observerCallbacks = [];

        var notifyObservers = function () {
            angular.forEach(observerCallbacks, function (callback) {
                callback();
            });
        };

        self.registerAccountObserver = function (callback) {
            observerCallbacks.push(callback);
        }

        self.setAccountName = function (name) {
            self.accountName = name;
            $localStorage.account = { 'name': name };
            notifyObservers();
        }

        self.accountName = $localStorage.account ? $localStorage.account.name : null;

        return self;
    }]);
})();