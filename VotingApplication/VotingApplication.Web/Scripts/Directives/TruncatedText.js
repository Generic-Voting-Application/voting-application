(function () {
    'use strict';

    angular
        .module('GVA.Common')
        .directive('truncatedText', truncatedText);

    truncatedText.$inject = ['$parse'];

    function truncatedText() {

        function truncateText(text, limit) {

            var words = text.split(' ');

            var truncatedtext = words.reduce(function (prev, curr) {
                if (curr.length + prev.length >= limit) {
                    return prev;
                } else {
                    return prev + ' ' + curr;
                }
            });

            return truncatedtext;
        }

        function link(scope) {

            activate();

            function activate() {

                scope.maxChars = parseInt(scope.maxChars) || 160;
                scope.fullText = '';
                scope.truncatedText = '';

                scope.$watch('ngModel', function () {
                    scope.fullText = scope.ngModel;
                    if (scope.fullText) {
                        scope.truncatedText = truncateText(scope.fullText, scope.maxChars);
                        scope.truncated = scope.fullText !== scope.truncatedText;
                    } else {
                        scope.truncated = false;
                    }
                });
            }
        }

        return {
            restrict: 'A',
            scope: {
                ngModel: '=',
                maxChars: '@',
            },
            link: link,
            template: '<span ng-if="!truncated">{{fullText}}</span>' +
                      '<span ng-if="truncated">{{truncatedText}}</span>' +
                      '<span ng-show="truncated">... <a style="cursor:pointer" ng-click="truncated=false">Show More</a></span>'
        };
    }
})();