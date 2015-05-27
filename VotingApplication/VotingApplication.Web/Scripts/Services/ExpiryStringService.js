(function () {
    'use strict';

    angular
        .module('GVA.Voting')
        .factory('ExpiryStringService', ExpiryStringService);


    function ExpiryStringService() {

        var service = {
            timeStringForExpiry: calculateTimeStringForExpiryDate

        };

        return service;


        function calculateTimeStringForExpiryDate(expiryDateUtc, callback) {
            if (expiryDateUtc < moment.utc()) {
                return 'Poll closed';
            }

            var timeString;

            var secondsToExpiry = expiryDateUtc.diff(moment.utc(), 'seconds');

            //NB: Months is an approximation, we don't need to be absolutely exact here.
            if (secondsToExpiry > 60 * 60 * 24 * 28) {
                timeString = formattedTimeString(expiryDateUtc, 60 * 60 * 24 * 28, 'month', callback);
            }
            else if (secondsToExpiry > 60 * 60 * 24) {
                timeString = formattedTimeString(expiryDateUtc, 60 * 60 * 24, 'day', callback);
            }
            else if (secondsToExpiry > 60 * 60) {
                timeString = formattedTimeString(expiryDateUtc, 60 * 60, 'hour', callback);
            }
            else if (secondsToExpiry > 60) {
                timeString = formattedTimeString(expiryDateUtc, 60, 'minute', callback);
            }
            else {
                timeString = formattedTimeString(expiryDateUtc, 1, 'second', callback);
            }

            return 'Poll closes in: ' + timeString;
        }

        function formattedTimeString(expiryDateUtc, timeUnit, timeUnitName, callback) {
            var secondsToExpiry = Math.floor(expiryDateUtc.diff(moment.utc(), 'seconds'));

            var timeValue = Math.floor(secondsToExpiry / timeUnit);
            timeUnitName = (timeValue === 1) ? timeUnitName : timeUnitName + 's';

            var timeString = timeValue + ' ' + timeUnitName;

            //Update at the next point we expect a change
            var timeoutTime = 1000 * (secondsToExpiry - 1 - timeValue * timeUnit);
            setTimeout(callback, timeoutTime, expiryDateUtc);

            return timeString;
        }
    }
})();
