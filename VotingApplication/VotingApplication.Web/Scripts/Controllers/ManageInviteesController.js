(function () {
    'use strict';

    angular
        .module('GVA.Creation')
        .controller('ManageInviteesController', ManageInviteesController);

    ManageInviteesController.$inject = ['$scope', '$routeParams', '$location', 'ManageService', 'RoutingService'];


    function ManageInviteesController($scope, $routeParams, $location, ManageService, RoutingService) {

        $scope.poll = ManageService.poll;
        $scope.manageId = $routeParams.manageId;

        $scope.emailUpdated = emailUpdated;
        $scope.addInvitee = addInvitee;
        $scope.deletePendingVoter = deletePendingVoter;
        $scope.deleteInvitedVoter = deleteInvitedVoter;
        $scope.sendInvitations = sendInvitations;
        $scope.inviteString = '';

        $scope.saveChanges = updatePoll;
        $scope.discardChanges = returnToManage;

        $scope.isSaving = false;

        $scope.pendingUsers = [];
        $scope.invitedUsers = [];

        activate();

        var splitterTest = /[\n\s;>]+/;
        var emailRegex = /[\w._%+-]+@\w+(\.\w+)+/;

        function emailUpdated() {
            if (hasTerminatingCharacter($scope.inviteString)) {
                var allEmails = $scope.inviteString.trimLeft().split(splitterTest);

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

        function deletePendingVoter(pending) {
            var indexOfInvitee = $scope.pendingUsers.indexOf(pending);
            $scope.pendingUsers.splice(indexOfInvitee, 1);
        }

        function deleteInvitedVoter(invitee) {
            var indexOfInvitee = $scope.invitedUsers.indexOf(invitee);
            $scope.invitedUsers.splice(indexOfInvitee, 1);
        }

        function addInvitee(invitee) {
            if (!invitee.match(emailRegex)) {
                return;
            }

            var allEmails = $scope.pendingUsers.concat($scope.invitedUsers);
            // Avoid duplicate invitations
            var existingEmails = allEmails.filter(function (d) {
                return (d.Email === invitee);
            });

            if (existingEmails.length === 0) {
                var newInvitee = { Email: invitee, EmailSent: false };
                $scope.pendingUsers.push(newInvitee);
            }


            // if inviteString.endsWith(invitee). Curse your obsfucation, Javascript!
            if ($scope.inviteString.indexOf(invitee, $scope.inviteString.length - invitee.length) !== 1) {
                $scope.inviteString = '';
            }
            else {
                $scope.inviteString = $scope.inviteeString.split(invitee)[1];
            }
        }

        function hasTerminatingCharacter(value) {
            return splitterTest.test(value);
        }

        function sendInvitations() {
            $scope.sendingInvitations = true;

            var existingInvitations = $scope.Invitations.map(function (d) {
                d.SendInvitation = false;
                return d;
            });

            var newInvitations = $scope.pendingUsers.map(function (d) {
                d.SendInvitation = true;
                return d;
            });

            var invitations = existingInvitations.concat(newInvitations);

            ManageService.sendInvitations($scope.manageId, invitations, function () {
                $scope.sendingInvitations = false;

                // We don't just do ManageService.getPoll, because we want to maintain any deleted "Invited" voters which have not yet been saved
                $scope.invitedUsers = $scope.invitedUsers.concat($scope.pendingUsers);
                $scope.pendingUsers = [];
            }, function () {
                $scope.sendingInvitations = false;
            });
        }

        function updatePoll() {
            // Apply pending change
            addInvitee($scope.inviteString);

            var invitations = $scope.invitedUsers.concat($scope.pendingUsers);

            ManageService.sendInvitations($scope.manageId, invitations, function () {
                returnToManage();
            });
        }

        function filterUsersByPending() {
            $scope.pendingUsers = [];
            $scope.invitedUsers = [];

            if ($scope.Invitations === undefined) {
                return;
            }

            for (var i = 0; i < $scope.Invitations.length; i++) {
                var voter = $scope.Invitations[i];

                if (voter.Email === null) {
                    continue;
                }
                
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
            ManageService.getInvitations($routeParams.manageId, function (data) {
                $scope.Invitations = data;
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
