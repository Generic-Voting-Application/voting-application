function AdminViewModel() {
    var self = this;

    self.votes = ko.observableArray();

    $(document).ready(function () {

        self.resetVotes = function () {
            $.ajax({
                type: 'DELETE',
                url: "/api/vote",

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
                url: '/api/vote?id=' + data.Id,
                contentType: 'application/json',

                success: function () {
                    self.populateVotes();
                }
            });
        }

        self.populateVotes = function () {
            $.ajax({
                type: 'GET',
                url: "/api/vote",

                success: function (data) {
                    //Replace contents of self.votes with 'data'
                    self.votes(data);
                }
            });
        }

        $("#reset-votes").click(self.resetVotes);
        self.populateVotes();
    });
}

ko.applyBindings(new AdminViewModel());
