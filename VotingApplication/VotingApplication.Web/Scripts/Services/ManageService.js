(function () {
    angular
        .module('GVA.Creation')
        .factory('ManageService', ManageService);


    ManageService.$inject = ['$location', '$http', '$routeParams', '$localStorage'];

    function ManageService($location, $http, $routeParams, $localStorage) {
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

            $http.get('/api/manage/' + manageId)
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

            $http.get('/api/poll/' + pollId + '/vote')
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
            var request = $http.get('/api/manage/' + manageId + '/vote');

            return request;
        };

        self.setVisited = function (manageId) {
            $localStorage[manageId] = { visited: true };
        };

        self.getVisited = function (manageId) {
            if (!$localStorage[manageId]) {
                return false;
            }
            return $localStorage[manageId].visited;
        };

        self.sendInvitations = function (manageId, callback, failureCallback) {
            $http.post('/api/manage/' + manageId + '/invitation')
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

        self.resetAllVotes = function (manageId) {
            var request = $http.delete('/api/manage/' + manageId + '/voters');

            return request;
        };

        self.resetBallot = function (manageId, ballotManageGuid) {
            var request = $http.delete('api/manage/' + manageId + '/voters/' + ballotManageGuid);

            return request;
        };

        return self;
    }
})();
