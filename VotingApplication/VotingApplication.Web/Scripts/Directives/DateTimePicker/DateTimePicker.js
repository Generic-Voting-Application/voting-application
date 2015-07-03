(function () {
    'use strict';

    angular
        .module('VoteOn-Components')
        .directive('dateTimePicker', dateTimePicker);


    function dateTimePicker() {
        
        function link(scope) {

            scope.displayDate = moment();
            scope.weekdays = moment.weekdaysMin();

            scope.advanceDisplayDate = advanceDisplayDate;
            scope.selectNow = selectNow;
            scope.selectDate = selectDate;
            scope.formatModelDate = formatModelDate;
            scope.advanceMinute = advanceMinute;
            scope.advanceHour = advanceHour;
            scope.minutes = 0;
            scope.hours = 0;
            
            function updateDisplay(displayDate, selectedDate) {
                displayDate = displayDate ? moment(displayDate) : moment();
                selectedDate = selectedDate ? moment(selectedDate) : null;

                scope.calendarRows = calculateRowDates(displayDate, selectedDate);
                scope.displayMonth = displayDate.format('MMM');

                scope.minutes = selectedDate ? selectedDate.minutes() : 0;
                scope.hours = selectedDate ? selectedDate.hours() : 0;
            }

            function selectNow() {
                scope.ngModel = moment().toDate();
                scope.displayDate = moment();
            }

            function selectDate(date) {
                scope.ngModel = moment(date).toDate();
                scope.displayDate = moment(date);
            }

            function advanceDisplayDate(amount) {
                changeDisplayDate(moment(scope.displayDate).add(amount, 'month'));
            }

            function changeDisplayDate(date) {
                scope.displayDate = moment(date);
                updateDisplay(scope.displayDate, scope.ngModel);
            }

            function formatModelDate() {
                return moment(scope.ngModel).format('lll');
            }

            function advanceMinute(amount) {
                scope.ngModel = moment(scope.ngModel).add(amount, 'minute');
            }

            function advanceHour(amount) {
                scope.ngModel = moment(scope.ngModel).add(amount, 'hour');
            }

            scope.$watch('ngModel', function () {
                updateDisplay(scope.displayDate, scope.ngModel);
            });
        }

        function calculateRowDates(displayDate, selectedDate) {

            var rows = [];

            var startDateOfMonth = displayDate.date(1);

            var cellCounter = 0;
            var rowCounter = 0;

            var dateCounter = moment(startDateOfMonth).subtract(startDateOfMonth.day(), 'day');

            rows[rowCounter] = [];

            for (var i = 0; i < 42; i++) {

                if (!rows[rowCounter]) {
                    rows[rowCounter] = [];
                }

                rows[rowCounter][cellCounter] = {
                    dayOfMonth: dateCounter.date(),
                    date: moment(dateCounter),
                    selected: dateCounter.isSame(selectedDate, 'day'),
                    inMonth: dateCounter.isSame(displayDate, 'month')
                };

                dateCounter.add(1, 'days');

                if (cellCounter % 7 === 6) {
                    rowCounter++;
                }
                cellCounter = (cellCounter + 1) % 7;
            }

            return rows;
        }

        return {
            restrict: 'E',
            replace: true,
            templateUrl: '/Scripts/Directives/DateTimePicker/DateTimePicker.html',
            scope: {
                ngModel: '='
            },
            link: link
        };
    }
})();
