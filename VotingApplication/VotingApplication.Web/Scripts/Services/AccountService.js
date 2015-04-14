(function () {
    'use strict';

    angular
        .module('GVA.Common')
        .factory('AccountService', AccountService);

    AccountService.$inject = ['$localStorage', '$http', 'ngDialog', '$q'];

    function AccountService($localStorage, $http, ngDialog, $q) {

        var self = this;

        var observerCallbacks = [];

        var notifyObservers = function () {
            angular.forEach(observerCallbacks, function (callback) {
                callback();
            });
        };

        self.account = $localStorage.account;

        self.registerAccountObserver = function (callback) {
            observerCallbacks.push(callback);
        };

        self.clearAccount = function () {
            self.account = null;
            delete $localStorage.account;
            notifyObservers();
        };

        self.login = login;

        self.registerAccountAndLogin = registerAccountAndLogin;

        self.forgotPassword = function (email) {
            return $http({
                method: 'POST',
                url: '/api/Account/ForgotPassword',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify({
                    Email: email
                })
            });
        };

        self.resetPassword = function (email, code, password, confirmPassword) {
            return $http({
                method: 'POST',
                url: '/api/Account/ResetPassword',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify({
                    Email: email,
                    Code: code,
                    Password: password,
                    ConfirmPassword: confirmPassword
                })
            });
        };

        self.openLoginDialog = function (scope) {
            ngDialog.open({
                template: '../Routes/AccountLogin',
                controller: 'AccountLoginController',
                'scope': scope
            });
        };

        self.openRegisterDialog = function (scope) {
            ngDialog.open({
                template: '../Routes/AccountRegister',
                controller: 'AccountRegisterController',
                'scope': scope
            });
        };

        return self;

        function login(email, password) {
            var deferred = $q.defer();

            getAccessToken(email, password)
                .then(function (response) { setAccount(response.data.access_token, email); })
                .then(function () { deferred.resolve(); })
                .catch(function () { deferred.reject(); });

            return deferred.promise;
        }

        function registerAccountAndLogin(email, password) {
            return register(email, password)
                .then(function () { return login(email, password); });
        }

        function register(email, password) {
            return $http({
                method: 'POST',
                url: '/api/Account/Register',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify({
                    Email: email,
                    Password: password
                })
            });
        }

        function getAccessToken(email, password) {
            return $http({
                method: 'POST',
                url: '/Token',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                transformRequest: function (obj) {
                    var str = [];
                    for (var p in obj) {
                        if (obj.hasOwnProperty(p)) {
                            str.push(encodeURIComponent(p) + '=' + encodeURIComponent(obj[p]));
                        }
                    }
                    return str.join('&');
                },
                data: {
                    grant_type: 'password',
                    username: email,
                    password: password
                }
            });
        }

        function setAccount(token, email) {
            var account = { 'token': token, 'email': email };
            self.account = account;
            $localStorage.account = account;

            notifyObservers();
        }
    }
})();