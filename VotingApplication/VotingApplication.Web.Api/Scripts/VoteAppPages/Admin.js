function AdminViewModel() {
    var self = this;
    var sessionId = 0;

    self.votes = ko.observableArray();
    self.sessions = ko.observableArray();
    self.options = ko.observableArray();
    self.selectedDeleteOptionId = null;
    self.currentSession = null;
    self.optionSetName = ko.observable();

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
    }

    self.deleteVote = function (data, event) {
        $.ajax({
            type: 'DELETE',
            url: '/api/session/' + sessionId + '/vote?id=' + data.Id,
            contentType: 'application/json',

            success: function () {
                self.populateVotes();
            }
        });
    }

    self.populateSession = function () {
        $.ajax({
            type: 'GET',
            url: 'api/session/' + sessionId,

            success: function (data) {
                self.currentSession = data;
                self.optionSetName(data.OptionSet.Name);
                self.options(data.OptionSet.Options);
            }
        })
    }

    self.populateOptions = function() {
        $.ajax({
            type: 'GET',
            url: 'api/session/' + sessionId,

            success: function (data) {
                self.options(data.OptionSet.Options);
            }
        })
    }

    self.populateVotes = function () {
        $.ajax({
            type: 'GET',
            url: "/api/session/" + sessionId + "/vote",

            success: function (data) {
                //Replace contents of self.votes with 'data'
                self.votes(data);
            }
        });
    }

    self.submitSession = function () {
        sessionId = $("#session-select").val();
        window.location = "?session=" + sessionId;
    }

    self.allSessions = function () {
        $.ajax({
            type: 'GET',
            url: '/api/session',

            success: function (data) {
                self.sessions(data);
            }
        })
    }

    self.deleteOption = function (data, event) {
        var matchingOption = self.options().filter(function (x) { return x.Id == data.Id })[0];
        var matchingIndex = self.options().indexOf(matchingOption);
        self.options().splice(matchingIndex, 1);

        self.createOptionSet(self.publishSession);
    }

    self.publishSession = function () {
        $.ajax({
            type: 'POST',
            url: '/api/session/' + sessionId,
            contentType: 'application/json',
            data: self.currentSession,

            success: function () {
                self.populateOptions();
            }
        });
    }

    self.createOptionSet = function (callback) {
        $.ajax({
            type: 'POST',
            url: '/api/optionset/',
            contentType: 'application/json',
            data: JSON.stringify({
                Name: $("#optionset-name").val(),
                Options: self.options
            }),

            success: function (data) {
                self.currentSession.OptionSetId = data
                if (callback) { callback(); }
            }
        });
    }

    $(document).ready(function () {
        sessionId = getSessionId();

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

        $("#reset-votes").click(self.resetVotes);
        $("#save-options-as").click(self.createOptionSet);
    });
}

ko.applyBindings(new AdminViewModel());
