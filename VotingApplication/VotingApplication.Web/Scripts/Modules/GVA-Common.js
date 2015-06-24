(function () {
    'use strict';

    angular
        .module('GVA.Common', ['ngDialog', 'ngStorage', 'ngRoute'])
        .config(['$httpProvider', function ($httpProvider) {
            $httpProvider.interceptors.push('ErrorInterceptor');
        }])

        .constant('Errors',
            {
                PollNotFound: { Id: 1, Text: 'Poll not found.' },

                NotAllowed: { Id: 2, Text: 'You are not allowed to access this.' },
                PollInviteOnlyNoToken: { Id: 21, Text: 'This poll is invite only. To vote, you need to be invited by the poll creator.' },

                IncorrectPollOrder: { Id: 3, Text: 'Incorrect poll route taken.'}
            });
})();
