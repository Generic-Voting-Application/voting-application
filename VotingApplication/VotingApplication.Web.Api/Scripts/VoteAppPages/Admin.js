function AdminViewModel() {
    var self = this;
    var sessionId = 0;

    self.votes = ko.observableArray();
    self.sessions = ko.observableArray();
    self.options = ko.observableArray();
    self.selectedDeleteOptionId = null;
    self.currentSession = null;
    self.sessionName = ko.observable();

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
                self.sessionName(data.Name);
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
        $.ajax({
            type: 'DELETE',
            url: '/api/option?id=' + data.Id,
            contentType: 'application/json',

            success: function () {
                self.populateOptions();
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
    });
}

ko.applyBindings(new AdminViewModel());
