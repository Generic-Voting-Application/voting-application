function ResultViewModel() {
    var self = this;

    self.votes = ko.observableArray();

    self.countVotes = function(voteArray)
    {
        var totalCounts = [];
        voteArray.forEach(function (vote) {
            var optionName = vote.Option.Name;
            var voter = vote.User.Name;

            // Find a vote with the same Option.Name, if it exists.
            var existingOption = totalCounts.filter(function (vote) { return vote.Name == optionName; }).pop();

            if (existingOption) {
                existingOption.Count++;
                existingOption.Voters.push(voter);
            }
            else {
                totalCounts.push({
                    Name: optionName,
                    Count: 1,
                    Voters: [voter]
                });
            }
        });
        return totalCounts;
    }

    $(document).ready(function () {
        // Get all options
        $.ajax({
            type: 'GET',
            url: "/api/vote",

            success: function (data) {
                var groupedVotes = self.countVotes(data);

                groupedVotes.forEach(function (vote) {
                    self.votes.push(vote);
                });
            }
        });
    });
}

ko.applyBindings(new ResultViewModel());
