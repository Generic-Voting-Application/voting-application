(function () {
    angular
        .module('GVA.Creation')
        .controller('ManageVotersController', ManageVotersController);

    ManageVotersController.$inject = ['$scope', '$routeParams', '$location', 'ManageService'];


    function ManageVotersController($scope, $routeParams, $location, ManageService) {

        $scope.poll = ManageService.poll;
        $scope.manageId = $routeParams.manageId;

        $scope.emailUpdated = emailUpdated;
        $scope.deleteVoter = deleteVoter;
        $scope.sendInvitations = sendInvitations;
        $scope.inviteString = '';

        $scope.updatePoll = updatePoll;
        $scope.return = returnToManage;

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
                        return { Email: d.match(emailRegex)[0] };
                    });

                $scope.poll.Voters = $scope.poll.Voters.concat(newEmails);
                $scope.inviteString = remainingText;
                $scope.$apply();
            }
        }

        function deleteVoter(voter) {
            var indexOfVoter = $scope.poll.Voters.indexOf(voter);

            $scope.poll.Voters.splice(indexOfVoter, 1);
        }

        function sendInvitations() {
            // Parse anything still in the field
            $scope.inviteString += ';';
            emailUpdated();

            updatePoll();
            returnToManage();
        }

        function hasTerminatingCharacter(value) {
            return splitterTest.test(value);
        }

        function updatePoll() {
            ManageService.updatePoll($routeParams.manageId, $scope.poll, function () {
                ManageService.getPoll($scope.manageId);
            });
        }

        function returnToManage() {
            $location.path('Manage/' + $scope.manageId);
        }

        function activate() {
            ManageService.registerPollObserver(function() {
                $scope.poll = ManageService.poll;
            });

            var inputField = document.getElementById('new-voter');
            inputField.addEventListener('keydown', function (e) {
                if (e.keyCode === 13) { // User pressed "return key"
                    $scope.inviteString += '\n';
                    emailUpdated();
                }
            });
        }
    }
})();
