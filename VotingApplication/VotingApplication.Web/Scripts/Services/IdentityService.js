(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.factory('IdentityService', ['$rootScope', '$location', '$http', '$localStorage', 'TokenService', function ($rootScope, $location, $http, $localStorage, TokenService) {

        var self = this;

        var observerCallbacks = [];

        var notifyObservers = function () {
            angular.forEach(observerCallbacks, function (callback) {
                callback();
            });
        };

        self.registerIdentityObserver = function (callback) {
            observerCallbacks.push(callback);
        }

        self.setIdentityName = function (name) {
            self.identityName = name;
            $localStorage.identity = { 'name': name };
            notifyObservers();
        }

        self.identityName = $localStorage.identity ? $localStorage.identity.name : null;

        return self;
    }]);
})();