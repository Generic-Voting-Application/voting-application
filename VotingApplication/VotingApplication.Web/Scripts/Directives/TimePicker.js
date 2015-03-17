(function () {
    angular
        .module('GVA.Creation')
        .directive('timePicker', timePicker);

    function timePicker() {

        var modelDirty = false;

        function link(scope, element, attrs) {

            scope.formatTime = formatTime;

            scope.moveHour = moveHour;
            scope.moveMinute = moveMinute;

            activate();

            function formatTime() {
                return moment(scope.ngModel).format('HH : mm');
            }

            function moveHour(offset) {
                scope.ngModel = moment(scope.ngModel).add(offset, 'hours');
            }

            function moveMinute(offset) {
                scope.ngModel = moment(scope.ngModel).add(offset, 'minutes');
            }

            function activate() {
                scope.$watch("ngModel", function () {
                    if (modelDirty) {
                        scope.update();
                        modelDirty = false;
                    }
                });
            }
        }

        return {
            restrict: 'E',
            scope: {
                ngModel: '=',
                update: '&'
            },
            link: link,
            templateUrl: '../Routes/TimePicker'
        }
    }

})();
