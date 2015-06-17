
(function () {
    'use strict';

    // What is a humidor, and how does it relate to errors?
    // Well obviously we want to keep our errors at a nice constant temperature and humidity,
    // so we'll store them in a humidor. Or I couldn't think of a proper name.
    // Feel free to change the name if you come up with something that makes sense as an html
    // element, as well as imparting the sense that it's actually the whole page.
    angular
        .module('GVA.Voting')
        .directive('errorHumidor', errorHumidor);

    function errorHumidor() {

        return {
            restrict: 'E',
            replace: true,
            templateUrl: '/Scripts/Directives/ErrorHumidor.html',
            scope: {
                errorText: '@'
            }
        };
    }
})();