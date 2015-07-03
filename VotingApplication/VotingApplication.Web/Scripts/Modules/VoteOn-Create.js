(function () {
    'use strict';

    angular
        .module('VoteOn-Create', ['ngMaterial', 'ngMessages', 'VoteOn-Common', 'VoteOn-Components'])
        .config(function ($mdThemingProvider) {
            $mdThemingProvider.theme('default')
              .primaryPalette('indigo')
              .accentPalette('indigo');

            $mdThemingProvider.theme('default-dark')

              .primaryPalette('indigo')
               .accentPalette('blue')
              .dark();
        })
        .constant('Errors',
            {
                PollNotFound: { Id: 1, Text: 'Poll not found.' },

                NotAllowed: { Id: 2, Text: 'You are not allowed to access this.' },
                PollInviteOnlyNoToken: { Id: 21, Text: 'This poll is invite only. To vote, you need to be invited by the poll creator.' },

                IncorrectPollOrder: { Id: 3, Text: 'Incorrect poll route taken.' }
            }
        );
})();
