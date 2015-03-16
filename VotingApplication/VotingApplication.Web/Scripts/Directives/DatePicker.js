(function () {
    angular
        .module('GVA.Creation')
        .directive('datePicker', datePicker);

    function datePicker() {

        function link(scope, element, attrs) {
            scope.weekdays = moment.weekdaysShort();
            scope.setDate = setDate;

            activate();


            function setDate(date) {
                scope.ngModel = date.toDate();
                scope.$apply();
            }

            function activate() {
                scope.$watch("ngModel", function () {
                    var modelDate = moment(scope.ngModel);
                    scope.rows = calculateRowDates(modelDate);
                });
            }
        }

        function calculateRowDates(date) {

            rows = [];

            var startDateOfMonth = date.date(1);
            var daysInMonth = date.daysInMonth();
            var firstDayOfMonth = startDateOfMonth.day();

            var cellCounter = firstDayOfMonth;
            var rowCounter = 0;

            // Fill up empty date cells
            rows[0] = [];

            for (a = 0; a < cellCounter; a++) {
                rows[0][a] = { date: '' };
            }

            for (i = 1; i <= daysInMonth; i++) {

                if (!rows[rowCounter]) {
                    rows[rowCounter] = [];
                }

                rows[rowCounter][cellCounter] = { date: i };

                rowCounter = Math.floor((firstDayOfMonth + i) / 7);
                cellCounter = (cellCounter + 1) % 7;

            }

            return rows;
        }

        return {
            restrict: 'E',
            scope: {
                ngModel: '=',
            },
            link: link,
            templateUrl: '../Routes/DatePicker'
        }
    }

})();
