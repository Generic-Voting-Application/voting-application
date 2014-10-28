function AdminViewModel() {
    var self = this;
    $(document).ready(function () {

        self.resetVotes = function () {
            $.ajax({
                type: 'DELETE',
                url: "/api/vote",

                success: function() {
                    $("#reset-votes").attr('disabled', 'disabled');
                    $("#reset-votes").text("Votes were reset");
                }
            });
        }

        $("#reset-votes").click(self.resetVotes);
    });
}

ko.applyBindings(new AdminViewModel());
