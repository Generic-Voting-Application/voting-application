(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.factory('IdentityService', ['$location', '$http', '$localStorage', 'ngDialog', 'TokenService',
            function ($location, $http, $localStorage, ngDialog, TokenService) {

                var self = this;

                var observerCallbacks = [];

                var notifyObservers = function () {
                    angular.forEach(observerCallbacks, function (callback) {
                        callback();
                    });
                };

                self.identityName = $localStorage.identity ? $localStorage.identity.name : null;

                self.registerIdentityObserver = function (callback) {
                    observerCallbacks.push(callback);
                }

                self.setIdentityName = function (name) {
                    self.identityName = name;
                    $localStorage.identity = { 'name': name };
                    notifyObservers();
                }

                self.clearIdentityName = function () {
                    self.identityName = null;
                    delete $localStorage.identity;
                    notifyObservers();
                }

                self.openLoginDialog = function (scope, callback) {
                    ngDialog.open({
                        template: 'Routes/LoginDialog',
                        controller: 'LoginController',
                        'scope': scope,
                        data: { 'callback': callback }
                    });
                }

                return self;
            }]);
})();