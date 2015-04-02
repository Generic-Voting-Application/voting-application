(function () {
    "use strict";

    angular
        .module('GVA.Creation')
        .directive('datePicker', datePicker);

    function datePicker() {

        var displayDate = moment();
        var selectedDate = null;
        var modelDirty = false;

        function link(scope) {
            scope.weekdays = moment.weekdaysShort();

            scope.setModelDate = setModelDate;
            scope.moveMonth = moveMonth;

            activate();

            function setModelDate(date) {
                modelDirty = true;
                scope.ngModel = date.toDate();
            }

            function moveMonth(offset) {
                displayDate.add(offset, 'months');
                updateDisplay(displayDate);
            }

            function updateDisplay(date) {
                scope.rows = calculateRowDates(date, scope.filter());
                scope.displayDate = date.format('MMMM YYYY');
            }

            function activate() {
                scope.$watch('ngModel', function () {
                    selectedDate = scope.ngModel ? moment(scope.ngModel) : moment();
                    displayDate = moment(selectedDate);
                    updateDisplay(displayDate);
                    if (scope.update && modelDirty) {
                        scope.update();
                        modelDirty = false;
                    }
                });
            }
        }

        function calculateRowDates(date, filter) {

            var rows = [];

            var startDateOfMonth = date.date(1);
            var daysInMonth = date.daysInMonth();
            var firstDayOfMonth = startDateOfMonth.day();

            var cellCounter = firstDayOfMonth;
            var rowCounter = 0;
            var dateCounter = moment(startDateOfMonth);

            // Fill up empty date cells
            rows[rowCounter] = [];

            for (var a = 0; a < cellCounter; a++) {
                rows[rowCounter][a] = { date: '' };
            }

            for (var i = 1; i <= daysInMonth; i++) {

                if (!rows[rowCounter]) {
                    rows[rowCounter] = [];
                }

                var cellDate = moment(dateCounter);
                var startOfSelectedDate = moment(selectedDate).startOf('day');
                var startOfCellDate = moment(cellDate).startOf('day');
                var selected = startOfCellDate.isSame(startOfSelectedDate);

                if (filter && !filter(cellDate)) {
                    rows[rowCounter][cellCounter] = { date: cellDate, enabled: false };
                } else {
                    rows[rowCounter][cellCounter] = { date: cellDate, selected: selected, enabled: true };
                }

                dateCounter.add(1, 'days');

                rowCounter = Math.floor((firstDayOfMonth + i) / 7);
                cellCounter = (cellCounter + 1) % 7;
            }

            return rows;
        }

        return {
            restrict: 'E',
            scope: {
                ngModel: '=',
                update: '&?',
                filter: '&?'
            },
            link: link,
            templateUrl: '../Routes/DatePicker'
        };
    }

})();
