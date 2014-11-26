require(['jquery', 'knockout', 'Common'], function ($, ko, Common) {
    function AdminViewModel() {
        var self = this;
        var sessionId = 0;

        self.votes = ko.observableArray();
        self.sessions = ko.observableArray();
        self.options = ko.observableArray();
        self.selectedDeleteOptionId = null;
        self.currentSession = null;
        self.templateName = ko.observable();

        self.resetVotes = function () {
            $.ajax({
                type: 'DELETE',
                url: "/api/session/" + sessionId + "/vote",

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
                url: '/api/session/' + sessionId + '/vote?id=' + data.Id,
                contentType: 'application/json',

                success: function () {
                    self.populateVotes();
                }
            });
        };

        self.populateSession = function () {
            $.ajax({
                type: 'GET',
                url: 'api/session/' + sessionId,

                success: function (data) {
                    self.currentSession = data;
                    self.templateName(data.Name);
                    self.populateOptions();
                }
            });
        };

        self.populateOptions = function () {
            $.ajax({
                type: 'GET',
                url: 'api/session/' + sessionId + '/option',

                success: function (data) {
                    self.options(data);
                }
            });
        };

        self.populateVotes = function () {
            $.ajax({
                type: 'GET',
                url: "/api/session/" + sessionId + "/vote",

                success: function (data) {
                    //Replace contents of self.votes with 'data'
                    self.votes(data);
                }
            });
        };

        self.submitSession = function () {
            sessionId = $("#session-select").val();
            window.location = "?poll=" + sessionId;
        };

        self.allSessions = function () {
            $.ajax({
                type: 'GET',
                url: '/api/session',

                success: function (data) {
                    self.sessions(data);
                }
            });
        };

        self.deleteOption = function (data, event) {
            $.ajax({
                type: 'DELETE',
                url: '/api/session/' + sessionId + '/option/' + data.Id,
                contentType: 'application/json',

                success: function () {
                    self.populateOptions();
                    self.populateVotes();
                }
            });
        };

        self.publishSession = function () {
            $.ajax({
                type: 'POST',
                url: '/api/session/' + sessionId,
                contentType: 'application/json',
                data: JSON.stringify(self.currentSession),

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
            sessionId = Common.getPollId();

            if (!sessionId) {
                self.allSessions();
                $("#admin-panel").hide();
                $("#sessions").show();
                return;
            }

            $("#sessions").hide();
            $("#admin-panel").show();

            self.populateVotes();
            self.populateSession();
        });
    }

    ko.applyBindings(new AdminViewModel());
});