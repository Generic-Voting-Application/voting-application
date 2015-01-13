define('Manage', ['jquery', 'knockout', 'bootstrap', 'Common'], function ($, ko, bootstrap, Common) {
    return function ManageViewModel(manageId) {
        var self = this;

        self.votes = ko.observableArray();
        self.options = ko.observableArray();
        self.votingStrategy = ko.observable(false);
        self.pollId = ko.observable();

        self.selectedDeleteOptionId = null;

        self.invitationText = ko.observable("");

        self.getPollDetails = function () {
            $.ajax({
                type: 'GET',
                url: '/api/manage/' + manageId,

                success: function (data) {
                    self.options(data.Options);
                    self.pollId(data.UUID);
                },

                error: Common.handleError
            });
        };

        self.populateVotes = function () {
            $.ajax({
                type: 'GET',
                url: "/api/manage/" + manageId + "/vote",

                success: function (data) {
                    //Replace contents of self.votes with 'data'
                    self.votes(data);
                }
            });
        };

        self.resetVotes = function () {
            $.ajax({
                type: 'DELETE',
                url: "/api/manage/" + manageId + "/vote",

                success: function () {
                    $("#reset-votes").attr('disabled', 'disabled');
                    $("#reset-votes").text("Votes were reset");
                    self.populateVotes();
                },

                error: Common.handleError
            });
        };

        self.addOption = function () {
            //Don't submit without an entry in the name field
            if ($("#newName").val() === "") {
                return;
            }

            var newName = $("#newName").val();
            var newInfo = $("#newInfo").val();
            var newDescription = $("#newDescription").val();

            //Reset before posting, to prevent double posts.
            $("#newName").val("");
            $("#newDescription").val("");
            $("#newInfo").val("");

            $.ajax({
                type: 'POST',
                url: '/api/manage/' + manageId + '/option',
                contentType: 'application/json',

                data: JSON.stringify({
                    Name: newName,
                    Description: newDescription,
                    Info: newInfo
                }),

                success: function () {
                    self.getPollDetails();
                },

                error: Common.handleError
            });
        };

        self.deleteVote = function (data, event) {
            $.ajax({
                type: 'DELETE',
                url: '/api/manage/' + manageId + '/vote/' + data.Id,
                contentType: 'application/json',

                success: function () {
                    self.populateVotes();
                },

                error: Common.handleError
            });
        };

        self.deleteOption = function (data, event) {
            $.ajax({
                type: 'DELETE',
                url: '/api/manage/' + manageId + '/option/' + data.Id,
                contentType: 'application/json',

                success: function () {
                    self.getPollDetails();
                    self.populateVotes();
                },

                error: Common.handleError
            });
        };

        self.updatePoll = function () {

        };

        self.sendInvites = function () {
            var invites = self.invitationText().split('\n');

            $.ajax({
                type: 'POST',
                url: '/api/manage/' + manageId + '/invitation',
                contentType: 'application/json',

                data: JSON.stringify(invites),

                success: function () {
                    self.invitationText("");
                }
            });
        }

        self.initialise = function () {
            self.getPollDetails();
            self.populateVotes();

            ko.applyBindings(this);
        };
    };
});
