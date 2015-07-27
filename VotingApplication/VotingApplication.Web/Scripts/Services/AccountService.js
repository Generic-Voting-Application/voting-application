(function () {
    'use strict';

    angular
        .module('GVA.Common')
        .factory('AccountService', AccountService);

    AccountService.$inject = ['$localStorage'];

    function AccountService($localStorage) {

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
            if (self.account && new Date(self.account.expiry) < new Date()) {
                self.clearAccount();
            }
        };

        self.clearAccount = function () {
            self.account = null;
            delete $localStorage.account;
            notifyObservers();
        };


        return self;
    }
})();

(function () {
    'use strict';

    angular
        .module('VoteOn-Account')
        .factory('AccountService', AccountService);

    AccountService.$inject = ['$http', '$localStorage', '$q', '$timeout'];

    function AccountService($http, $localStorage, $q, $timeout) {

        var observerCallbacks = [];

        var service = {
            account: $localStorage.account,
            registerAccountObserver: registerAccountObserver,

            login: login,
            register: register,
            resendConfirmation: resendConfirmation,
            forgotPassword: forgotPassword,
            resetPassword: resetPassword,
            logout: logout
        };

        return service;

        function login(email, password) {

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
            })
            .then(function (response) {

                var accountToken = response.data.access_token;
                // If you try and parse the expiry date without a format, moment will warn that this is
                // deprecated (https://github.com/moment/moment/issues/1407)
                // The string without the day is valid and can be parsed, but we'll just include it in
                // the format string.
                var expiryDateUtc = moment(response.data['.expires'], 'ddd, DD MMM YYYY h:mm a');

                return setAccount(accountToken, email, expiryDateUtc);
            });
        }

        function setAccount(token, email, expiryDateUtc) {
            var deferred = $q.defer();

            var account = { 'email': email, 'token': token, 'expiryDateUtc': expiryDateUtc };

            $localStorage.account = account;
            service.account = account;

            // Ensure that we've actually saved the values before continuing. (see https://github.com/gsklee/ngStorage/issues/39)
            $timeout(function () { deferred.resolve(); }, 110);

            notifyObservers();

            return deferred.promise;
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

        function resendConfirmation(email) {
            return $http({
                method: 'POST',
                url: '/api/Account/ResendConfirmation?email=' + email,
                contentType: 'application/json; charset=utf-8'
            });
        }

        function forgotPassword(email) {
            return $http({
                method: 'POST',
                url: '/api/Account/ForgotPassword',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify({
                    Email: email
                })
            });
        }

        function resetPassword(email, password, resetToken) {
            return $http({
                method: 'POST',
                url: '/api/Account/ResetPassword',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify({
                    Email: email,
                    Password: password,
                    Code: resetToken
                })
            });
        }

        function logout() {
            delete $localStorage.account;
            service.account = null;

            notifyObservers();
        }

        function registerAccountObserver(callback) {
            observerCallbacks.push(callback);

            var account = service.account;
            if (account && moment.utc(account.expiryDateUtc).isBefore(moment.utc())) {
                logout();
            }
        }

        function notifyObservers() {
            angular.forEach(observerCallbacks, function (callback) {
                callback();
            });
        }
    }
})();