(function () {
    angular
        .module('GVA.Creation')
        .factory('ManageService', ManageService);


    ManageService.$inject = ['$location', '$http', '$routeParams'];

    function ManageService($location, $http, $routeParams) {
        var self = this;

        var observerCallbacks = [];

        var notifyObservers = function () {
            angular.forEach(observerCallbacks, function (callback) {
                callback();
            });
        };

        self.poll = null;

        self.registerPollObserver = function (callback) {

            if (self.poll == null) {
                self.getPoll($routeParams.manageId);
            }

            observerCallbacks.push(callback);
        };

        self.getPoll = function (manageId, callback, failureCallback) {

            if (!manageId) {
                return null;
            }

            $http({
                method: 'GET',
                url: '/api/manage/' + manageId
            })
                .success(function (data) {
                    self.poll = data;
                    notifyObservers();
                    if (callback) {
                        callback(data);
                    }
                })
                .error(function (data, status) {
                    if (failureCallback) {
                        failureCallback(data, status);
                    }
                });
        };

        self.updatePoll = function (manageId, poll, callback, failureCallback) {

            $http({
                method: 'PUT',
                url: '/api/manage/' + manageId,
                data: poll
            })
                .success(function (data) {
                    if (callback) {
                        callback(data);
                    }
                })
                .error(function (data, status) {
                    if (failureCallback) {
                        failureCallback(data, status);
                    }
                });

        };

        self.getVotes = function (pollId, callback, failureCallback) {

            $http({
                method: 'GET',
                url: '/api/poll/' + pollId + '/vote'
            })
                .success(function (data) {
                    if (callback) {
                        callback(data);
                    }
                })
                .error(function (data, status) {
                    if (failureCallback) {
                        failureCallback(data, status);
                    }
                });
        };

        self.getVoters = function (manageId) {
            var request = $http({
                method: 'GET',
                url: '/api/manage/' + manageId + '/vote'
            });

            return request;
        }

        return self;
    }
})();
