require(['jquery', 'knockout', 'Common'], function ($, ko, Common) {
    function AdminViewModel() {
        var self = this;
        var pollId = 0;

        self.votes = ko.observableArray();
        self.polls = ko.observableArray();
        self.options = ko.observableArray();
        self.selectedDeleteOptionId = null;
        self.currentPoll = null;
        self.templateName = ko.observable();

        self.resetVotes = function () {
            $.ajax({
                type: 'DELETE',
                url: "/api/poll/" + pollId + "/vote",

                success: function () {
                    $("#reset-votes").attr('disabled', 'disabled');
                    $("#reset-votes").text("Votes were reset");
                    self.populateVotes();
                }
            });
        };

        self.deleteVote = function (data, event) {
            $.ajax({
                type: 'DELETE',
                url: '/api/poll/' + pollId + '/vote?id=' + data.Id,
                contentType: 'application/json',

                success: function () {
                    self.populateVotes();
                }
            });
        };

        self.populatePoll = function () {
            $.ajax({
                type: 'GET',
                url: 'api/poll/' + pollId,

                success: function (data) {
                    self.currentPoll = data;
                    self.templateName(data.Name);
                    self.populateOptions();
                }
            });
        };

        self.populateOptions = function () {
            $.ajax({
                type: 'GET',
                url: 'api/poll/' + pollId + '/option',

                success: function (data) {
                    self.options(data);
                }
            });
        };

        self.populateVotes = function () {
            $.ajax({
                type: 'GET',
                url: "/api/poll/" + pollId + "/vote",

                success: function (data) {
                    //Replace contents of self.votes with 'data'
                    self.votes(data);
                }
            });
        };

        self.submitPoll = function () {
            pollId = $("#poll-select").val();
            window.location = "?poll=" + pollId;
        };

        self.allPolls = function () {
            $.ajax({
                type: 'GET',
                url: '/api/poll',

                success: function (data) {
                    self.polls(data);
                }
            });
        };

        self.deleteOption = function (data, event) {
            $.ajax({
                type: 'DELETE',
                url: '/api/poll/' + pollId + '/option/' + data.Id,
                contentType: 'application/json',

                success: function () {
                    self.populateOptions();
                    self.populateVotes();
                }
            });
        };

        self.publishPoll = function () {
            $.ajax({
                type: 'POST',
                url: '/api/poll/' + pollId,
                contentType: 'application/json',
                data: JSON.stringify(self.currentPoll),

                success: function () {
                    self.populateOptions();
                }
            });
        };

        self.createTemplate = function () {
            $.ajax({
                type: 'POST',
                url: '/api/template/',
                contentType: 'application/json',
                data: JSON.stringify({
                    Name: $("#template-name").val(),
                    Options: self.options()
                }),
            });
        };

        $(document).ready(function () {
            pollId = Common.getPollId();

            if (!pollId) {
                self.allPolls();
                $("#admin-panel").hide();
                $("#polls").show();
                return;
            }

            $("#polls").hide();
            $("#admin-panel").show();

            self.populateVotes();
            self.populatePoll();
        });
    }

    ko.applyBindings(new AdminViewModel());
});