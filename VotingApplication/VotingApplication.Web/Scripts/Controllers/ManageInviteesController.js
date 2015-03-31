(function () {
    angular
        .module('GVA.Creation')
        .controller('ManageInviteesController', ManageInviteesController);

    ManageInviteesController.$inject = ['$scope', '$routeParams', '$location', 'ManageService', 'RoutingService'];


    function ManageInviteesController($scope, $routeParams, $location, ManageService, RoutingService) {

        $scope.poll = ManageService.poll;
        $scope.manageId = $routeParams.manageId;

        $scope.emailUpdated = emailUpdated;
        $scope.addInvitee = addInvitee;
        $scope.deleteInvitee = deleteInvitee;
        $scope.sendInvitations = sendInvitations;
        $scope.inviteString = '';

        $scope.updatePoll = updatePoll;
        $scope.return = returnToManage;

        $scope.pendingUsers = [];
        $scope.invitedUsers = [];

        activate();

        var splitterTest = /[\n\s;>]+/;
        var emailRegex = /[\w._%+-]+@\w+(\.\w+)+/;

        function emailUpdated() {
            var lastCharacter = $scope.inviteString.slice(-1);
            if (hasTerminatingCharacter($scope.inviteString)) {
                var allEmails = $scope.inviteString.trimLeft().split(splitterTest);

                var remainingText = allEmails.slice(-1);
                var newEmails = allEmails.slice(0, -1);

                newEmails = newEmails
                    .filter(function (d) {
                        // Only add email-like tokens
                        return emailRegex.test(d);
                    }).map(function (d) {
                        // Parse to extract the email-like section. 
                        // E.g. Turns "Joe Bloggs <jbloggs@example.com>" into "jbloggs@example.com
                        return d.match(emailRegex)[0];
                    });

                for (var i = 0; i < newEmails.length; i++) {
                    addInvitee(newEmails[i]);
                }

                $scope.$apply();
            }
        }

        function deleteInvitee(invitee) {
            var matchingUser = $scope.poll.Voters.filter(function (d) {
                return d.Email === invitee.Email;
            });
            var indexOfInvitee = $scope.poll.Voters.indexOf(matchingUser[0]);

            $scope.poll.Voters.splice(indexOfInvitee, 1);
            updatePoll();
        }

        function addInvitee(invitee) {
            if (!invitee.match(emailRegex)) {
                return;
            }

            // Avoid duplicate invitations
            var existingEmails = $scope.poll.Voters.filter(function (d) {
                return (d.Email === invitee);
            });

            if (existingEmails.length === 0) {
                var newInvitee = { Email: invitee, EmailSent: false };
                $scope.poll.Voters.push(newInvitee);
                $scope.pendingUsers.push(newInvitee);
            }


            // if inviteString.endsWith(invitee). Curse your obsfucation, Javascript!
            if ($scope.inviteString.indexOf(invitee, $scope.inviteString.length - invitee.length) !== 1) {
                $scope.inviteString = '';
            }
            else {
                $scope.inviteString = $scope.inviteeString.split(invitee)[1];
            }

            updatePoll();
        }

        function hasTerminatingCharacter(value) {
            return splitterTest.test(value);
        }

        function sendInvitations() {
            ManageService.sendInvitations($routeParams.manageId, function() {
                ManageService.getPoll($scope.manageId);
            });
        }

        function updatePoll() {
            ManageService.updatePoll($routeParams.manageId, $scope.poll, function () {
                ManageService.getPoll($scope.manageId);
            });
        }

        function filterUsersByPending() {
            $scope.pendingUsers = [];
            $scope.invitedUsers = [];

            if ($scope.poll === null) {
                return;
            }

            for (var i = 0; i < $scope.poll.Voters.length; i++) {
                var voter = $scope.poll.Voters[i];
                
                if (voter.EmailSent) {
                    $scope.invitedUsers.push(voter);
                }
                else {
                    $scope.pendingUsers.push(voter);
                }
            }
        }

        function returnToManage() {
            RoutingService.navigateToManagePage($scope.manageId);
        }

        function activate() {
            ManageService.registerPollObserver(function () {
                $scope.poll = ManageService.poll;
                filterUsersByPending();
            });

            filterUsersByPending();

            var inputField = document.getElementById('new-invitee');
            inputField.addEventListener('keydown', function (e) {
                if (e.keyCode === 13) { // User pressed "return key"
                    $scope.inviteString += '\n';
                    emailUpdated();
                }
            });
        }
    }
})();
