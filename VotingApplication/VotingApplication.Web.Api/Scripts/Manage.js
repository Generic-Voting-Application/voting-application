require(['jquery', 'knockout', 'Common'], function ($, ko, Common) {
    function AdminViewModel() {
        var self = this;
        var manageId = 0;

        self.votes = ko.observableArray();
        self.options = ko.observableArray();
        self.selectedDeleteOptionId = null;

        self.resetVotes = function () {
            $.ajax({
                type: 'DELETE',
                url: "/api/manage/" + manageId + "/vote",

                success: function () {
                    $("#reset-votes").attr('disabled', 'disabled');
                    $("#reset-votes").text("Votes were reset");
                    self.populateVotes();
                }
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
                    self.populateOptions();
                }
            });
        };

        self.deleteVote = function (data, event) {
            $.ajax({
                type: 'DELETE',
                url: '/api/manage/' + manageId + '/vote/' + data.Id,
                contentType: 'application/json',

                success: function () {
                    self.populateVotes();
                }
            });
        };

        self.deleteOption = function (data, event) {
            $.ajax({
                type: 'DELETE',
                url: '/api/manage/' + manageId + '/option/' + data.Id,
                contentType: 'application/json',

                success: function () {
                    self.populateOptions();
                    self.populateVotes();
                }
            });
        };

        self.populateOptions = function () {
            $.ajax({
                type: 'GET',
                url: '/api/manage/' + manageId + '/option',

                success: function (data) {
                    self.options(data);
                }
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

        $(document).ready(function () {
            manageId = Common.getManageId();

            self.populateVotes();
            self.populateOptions();

            //Add option on pressing return key
            $("#newOptionRow").keypress(function (event) { Common.keyIsEnter(event, self.addOption); });
        });
    }

    ko.applyBindings(new AdminViewModel());
});