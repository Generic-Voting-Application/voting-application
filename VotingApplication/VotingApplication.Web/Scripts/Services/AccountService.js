(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.factory('AccountService', ['$localStorage', '$http', '$localStorage', 'ngDialog',
        function ($localStorage, $http, $localStorage, ngDialog) {

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

            self.setAccount = function (token) {
                $localStorage.account = { 'token': token };
                notifyObservers();
            }

            self.clearAccount = function () {
                delete $localStorage.account;
                notifyObservers();
            }

            self.getAccessToken = function (email, password, callback, failureCallback) {
                $http({
                    method: 'POST',
                    url: '/Token',
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                    transformRequest: function (obj) {
                        var str = [];
                        for (var p in obj)
                            str.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
                        return str.join("&");
                    },
                    data: {
                        grant_type: 'password',
                        username: email,
                        password: password
                    }
                })
                .success(function (data) { if (callback) { callback(data) } })
                .error(function (data, status) { if (failureCallback) { failureCallback(data, status) } });
            }

            self.register = function(email, password, callback, failureCallback) {
                $http({
                    method: 'POST',
                    url: '/api/Account/Register',
                    contentType: 'application/json; charset=utf-8',
                    data: JSON.stringify({
                        Email: email,
                        Password: password,
                    })
                })
               .success(function (data) { if (callback) { callback(data) } })
               .error(function (data, status) { if (failureCallback) { failureCallback(data, status) } });
            }

            self.openLoginDialog = function (scope, callback) {
                ngDialog.open({
                    template: '../Routes/AccountLogin',
                    controller: 'AccountLoginController',
                    'scope': scope,
                    data: { 'callback': callback }
                });
            }

            self.openRegisterDialog = function (scope, callback) {
                ngDialog.open({
                    template: '../Routes/AccountRegister',
                    controller: 'AccountRegisterController',
                    'scope': scope,
                    data: { 'callback': callback }
                });
            }

            return self;
        }]);
})();