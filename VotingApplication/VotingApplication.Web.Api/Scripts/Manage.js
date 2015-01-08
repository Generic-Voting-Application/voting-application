require(['jquery', 'knockout', 'bootstrap', 'Common'], function ($, ko, bootstrap, Common) {
    function AdminViewModel() {
        var self = this;
        var manageId = 0;

        self.votes = ko.observableArray();
        self.options = ko.observableArray();
        self.votingStrategy = ko.observable(false);

        self.selectedDeleteOptionId = null;

        var getPollDetails = function () {
            $.ajax({
                type: 'GET',
                url: '/api/manage/' + manageId,

                success: function (data) {
                    self.options(data.Options);
                },

                error: Common.handleError
            });
        };

        var populateVotes = function () {
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
                    populateVotes();
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
                    getPollDetails();
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
                    populateVotes();
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
                    getPollDetails();
                    populateVotes();
                },

                error: Common.handleError
            });
        };

        self.updatePoll = function () {

        }

        self.sendInvites = function () {
            var invites = $("#invites").val().split('\n');

            $.ajax({
                type: 'POST',
                url: '/api/manage/' + manageId + '/invitation',
                contentType: 'application/json',

                data: JSON.stringify({ Invitation: invites }),

                success: function () {
                    $("#invites").val("");
                }
            });
        }

        $(document).ready(function () {
            manageId = Common.getManageId();

            getPollDetails();
            populateVotes();

            // Select first tab
            $('#tabBar li a:first').tab('show')

            //Add option on pressing return key
            $("#newOptionRow").keypress(function (event) { Common.keyIsEnter(event, self.addOption); });
        });
    }

    ko.applyBindings(new AdminViewModel());
});