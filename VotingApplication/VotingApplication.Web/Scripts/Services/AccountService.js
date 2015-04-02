(function () {
    'use strict';

    angular
        .module('GVA.Common')
        .factory('AccountService', AccountService);

    AccountService.$inject = ['$localStorage', '$http', 'ngDialog'];

    function AccountService($localStorage, $http, ngDialog) {

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

        self.setAccount = function (token, email) {
            var account = { 'token': token, 'email': email };
            self.account = account;
            $localStorage.account = account;
            notifyObservers();
        };

        self.clearAccount = function () {
            self.account = null;
            delete $localStorage.account;
            notifyObservers();
        };

        self.getAccessToken = function (email, password) {
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
        };

        self.register = function (email, password) {
            return $http({
                method: 'POST',
                url: '/api/Account/Register',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify({
                    Email: email,
                    Password: password
                })
            });
        };

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

        self.openLoginDialog = function (scope, callback) {
            ngDialog.open({
                template: '../Routes/AccountLogin',
                controller: 'AccountLoginController',
                'scope': scope,
                data: { 'callback': callback }
            });
        };

        self.openRegisterDialog = function (scope, callback) {
            ngDialog.open({
                template: '../Routes/AccountRegister',
                controller: 'AccountRegisterController',
                'scope': scope,
                data: { 'callback': callback }
            });
        };

        return self;
    }
})();