(function () {
    'use strict';

    angular
        .module('VoteOn-Signaling')
        .factory('SignalService', SignalService);


    SignalService.$inject = ['Hub'];

    function SignalService(Hub) {

        var updateFunction = null;
        var hub = setUpHub();
        
        var service = {
            registerObserver: registerObserver
        };

        return service;

        function registerObserver(resourceId, updateCallback) {
            updateFunction = updateCallback;
            hub.promise.then(function () {
                hub.registerObserver(resourceId);
            });
        }

        function setUpHub() {
            var hubOptions = {};
            
            hubOptions.listeners = {
                'update': function () {
                    if (updateFunction) {
                        updateFunction();
                    }
                }
            };

            hubOptions.methods = ['registerObserver'];
            hubOptions.errorHandler = function (error) {
                console.error(error);
            };

            return new Hub('SignalHub', hubOptions);
        }
    }
})();