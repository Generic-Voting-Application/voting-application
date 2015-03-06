(function () {
    angular
        .module('GVA.Voting')
        .factory('ExpiryStringService',
        function () {
            var self = this;

            var formattedTimeString = function (expiryDate, timeUnit, timeUnitName, callback) {
                var secondsToExpiry = Math.floor((expiryDate - Date.now()) / 1000);

                var timeValue = Math.floor(secondsToExpiry / timeUnit);
                timeUnitName = (timeValue == 1) ? timeUnitName : timeUnitName + 's';

                var timeString = timeValue + ' ' + timeUnitName;

                //Update at the next point we expect a change
                var timeoutTime = 1000 * (secondsToExpiry - 1 - timeValue * timeUnit);
                setTimeout(callback, timeoutTime, expiryDate);

                return timeString;
            }

            self.timeStringForExpiry = function (expiryDate, callback) {
                if (expiryDate < Date.now()) {
                    return 'Poll closed';
                }

                var timeString;

                var secondsToExpiry = (expiryDate - Date.now()) / 1000;

                //NB: Months is an approximation, we don't need to be absolutely exact here.
                if (secondsToExpiry > 60 * 60 * 24 * 28) {
                    timeString = formattedTimeString(expiryDate, 60 * 60 * 24 * 28, 'month', callback);
                }
                else if (secondsToExpiry > 60 * 60 * 24) {
                    timeString = formattedTimeString(expiryDate, 60 * 60 * 24, 'day', callback);
                }
                else if (secondsToExpiry > 60 * 60) {
                    timeString = formattedTimeString(expiryDate, 60 * 60, 'hour', callback);
                }
                else if (secondsToExpiry > 60) {
                    timeString = formattedTimeString(expiryDate, 60, 'minute', callback);
                }
                else {
                    timeString = formattedTimeString(expiryDate, 1, 'second', callback);
                }

                return 'Poll closes in: ' + timeString;
            };

            return self;
        });
})();
