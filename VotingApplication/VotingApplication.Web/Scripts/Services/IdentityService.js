(function () {
    'use strict';

    angular
        .module('GVA.Voting')
        .factory('IdentityService', IdentityService);


    IdentityService.$inject = ['$localStorage', 'ngDialog'];

    function IdentityService($localStorage, ngDialog) {

        var self = this;

        var observerCallbacks = [];

        var notifyObservers = function () {
            angular.forEach(observerCallbacks, function (callback) {
                callback();
            });
        };

        self.identity = $localStorage.identity;

        self.registerIdentityObserver = function (callback) {
            observerCallbacks.push(callback);
        };

        self.setIdentityName = function (name) {
            var identity = { 'name': name };

            self.identity = identity;
            $localStorage.identity = identity;

            notifyObservers();
        };

        self.clearIdentityName = function () {
            self.identity = null;
            delete $localStorage.identity;

            notifyObservers();
        };

        self.openLoginDialog = function (scope, callback) {
            ngDialog.open({
                template: '../Routes/IdentityLogin',
                controller: 'IdentityLoginController',
                'scope': scope,
                data: { 'callback': callback }
            });
        };

        return self;
    }
})();

(function () {
    'use strict';

    angular
        .module('VoteOn-Poll')
        .factory('IdentityService', IdentityService);


    IdentityService.$inject = ['$localStorage'];

    function IdentityService($localStorage) {

        if ($localStorage.identity === undefined) {
            $localStorage.identity = { name: '' };
        }

        var service = {
            identity: $localStorage.identity
        };

        return service;
    }
})();