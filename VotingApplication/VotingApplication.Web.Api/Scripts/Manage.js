define('Manage', ['jquery', 'knockout', 'bootstrap', 'Common', 'Navbar'], function ($, ko, bootstrap, Common, Navbar) {
    return function ManageViewModel(manageId) {
        var self = this;

        self.votes = ko.observableArray();
        self.options = ko.observableArray();
        self.templates = ko.observableArray();
        self.pollId = ko.observable();
        self.templateId = ko.observable();

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


        self.populateTemplates = function () {
            if (!self.isSignedIn()) {
                return;
            }

            $.ajax({
                type: 'GET',
                url: '/api/poll',
                beforeSend: function (header) {
                    header.setRequestHeader("Authorization", "Bearer " + sessionStorage['creator_token']);
                },

                success: function (data) {
                    for (var i = 0; i < data.length; i++) {
                        var date = new Date(data[i].CreatedDate);
                        data[i].Name = data[i].Name + " (" + date.toDateString() + ")";
                    }
                    self.templates(data);
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

        self.cloneOptions = function () {
            // The ID of the poll to copy - Not the poll we are currently managing
            var chosenPollId = self.templateId();

            if (chosenPollId == undefined) {
                return;
            }

            $.ajax({
                type: 'GET',
                url: "/api/poll/" + chosenPollId + "/option",

                success: function (data) {
                    self.saveOptions(data);
                },

                error: Common.handleError
            });
        }

        self.saveOptions = function (options) {
            // We need to get the resulting list back so we have the correct IDs
            $.ajax({
                type: 'PUT',
                url: "/api/manage/" + manageId + "/option",
                contentType: 'application/json',
                data: JSON.stringify(options),
                success: function (data) {
                    self.options(data)
                },
                error: Common.handleError
            });
        }

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

        self.isSignedIn = function () {
            return Navbar.signedIn();
        }

        self.initialise = function () {
            $('[data-toggle="tooltip"]').tooltip({ html: true });

            self.getPollDetails();
            self.populateVotes();
            self.populateTemplates();

            ko.applyBindings(this);
        };
    };
});
