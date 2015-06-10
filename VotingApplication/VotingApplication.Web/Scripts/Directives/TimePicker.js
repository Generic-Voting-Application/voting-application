(function () {
    'use strict';

    angular
        .module('GVA.Manage')
        .directive('timePicker', timePicker);

    function timePicker() {

        var modelDirty = false;
        var time = moment();

        function link(scope) {

            scope.formatTime = formatTime;

            scope.moveHour = moveHour;
            scope.moveMinute = moveMinute;

            activate();

            function formatTime() {
                return time.format('HH : mm');
            }

            function moveHour(offset) {
                time.hours((time.hours() + offset + 24) % 24);
                modelDirty = true;
                scope.ngModel = time.toDate();
                scope.update();
            }

            function moveMinute(offset) {
                time.minutes((time.minutes() + offset + 60) % 60);
                modelDirty = true;
                scope.ngModel = time.toDate();
                scope.update();
            }

            function activate() {

                var debounce = null;

                scope.$watch('ngModel', function () {

                    // Lock minutes to multiples of 5
                    var roundedDate = scope.ngModel ? moment(scope.ngModel) : moment();
                    roundedDate.minutes(Math.ceil((roundedDate.minutes()) / 5) * 5);
                    
                    time = moment(roundedDate);

                    if (scope.update && modelDirty) {
                        if (scope.debounce) {
                            clearTimeout(debounce);
                            debounce = setTimeout(function () {
                                modelDirty = false;
                            }, parseInt(scope.debounce));
                        } else {
                            modelDirty = false;
                        }
                    }

                });
            }
        }

        return {
            restrict: 'E',
            scope: {
                ngModel: '=',
                update: '&?',
                debounce: '@'
            },
            link: link,
            templateUrl: '../Routes/TimePicker'
        };
    }

})();
