define(['jquery', 'knockout', 'jqueryUI', 'Common', 'jqueryTouch'], function ($, ko, jqueryUI, Common) {

    return function RankedVote(options) {

        self = this;
        self.options = ko.observableArray(options);
        self.selectedOptions = ko.observableArray();
        self.resultOptions = ko.observableArray();

        var selectOption = function (option) {
            var $option = $('#optionTable > tbody > tr').filter(function () {
                return $(this).attr('data-id') == option.Id;
            });

            $('#selectionTable > tbody').append($option.remove());
            self.selectedOptions.push(option)
        }

        var selectPickedOptions = function (pickedOptions) {
            self.selectedOptions.removeAll();

            pickedOptions.forEach(function (option) {
                var pickedOption = ko.utils.arrayFirst(self.options(), function (item) {
                    return item.Id == option.OptionId;
                });

                selectOption(pickedOption);
            });
        }

        var resetOptions = function () {
            $('#optionTable > tbody').append($('#selectionTable > tbody').remove('tr').children());
        }
        
        var sortByPollValue = function (a, b) {
            return a.PollValue - b.PollValue;
        }

        var sortByBallotCount = function (a, b) {
            return a.ballots.length - b.ballots.length;
        }

        var countVotes = function (votes) {
            var options = [];
            var orderedOptions = [];
            var ballots = [];
            var totalBallots = 0;
            var totalOptions = self.options().length;

            for (var k = 0; k < totalOptions; k++) {
                var optionId = self.options()[k].Id;
                options[k] = { Id: optionId, ballots: [] };
            }

            // Group votes into ballots (per user)
            votes.forEach(function (vote) {
                if (!ballots[vote.UserId]) {
                    ballots[vote.UserId] = [];
                    totalBallots++;
                }
                ballots[vote.UserId].push(vote);
            });

            // Start counting
            while (options.length > 1) {

                // Clear out all ballots from previous round
                options = options.map(function (d) {
                    d.ballots = [];
                    return d;
                });

                var availableOptions = options.map(function (d) { return d.Id; });

                // Sort the votes on the ballots and assign each ballot to first choice
                ballots.forEach(function (ballot) {
                    ballot.sort(sortByPollValue);
                    var availableChoices = ballot.filter(function (option) { return $.inArray(option.OptionId, availableOptions) != -1; })
                    if (availableChoices.length > 0) {
                        var firstChoiceId = availableChoices[0].OptionId
                        var firstChoiceOption = options.filter(function (option) { return option.Id == firstChoiceId })[0];
                        firstChoiceOption.ballots.push(ballot)
                    }
                });

                options.sort(sortByBallotCount);

                console.log(options);

                // End if we have a majority
                if (options[options.length - 1].ballots.length > totalBallots / 2) {
                    break;
                }

                // Remove all last place options
                var lastPlaceOption = options[0];
                var lastPlaceBallotCount = lastPlaceOption.ballots.length;

                var removedOptions = options.filter(function (d) { return d.ballots.length == lastPlaceBallotCount; });
                orderedOptions.push.apply(orderedOptions, removedOptions);

                options = options.filter(function (d) { return d.ballots.length > lastPlaceBallotCount; });
            }

            orderedOptions.push.apply(orderedOptions, options);
            orderedOptions.reverse();

            return orderedOptions;
        }

        var displayResults = function (votes) {
            var orderedResults = countVotes(votes);
            self.resultOptions.removeAll();
            for (var i = 0; i < orderedResults.length; i++) {

                var option = ko.utils.arrayFirst(self.options(), function (item) {
                    return item.Id == orderedResults[i].Id;
                });

                option.Rank = i + 1;

                self.resultOptions.push(option);
            }
        }

        self.doVote = function (data, event) {
            var userId = Common.currentUserId();
            var pollId = Common.getPollId();
            var token = Common.getToken();

            if (userId && pollId) {

                var selectionRows = $('#selectionTable > tbody > tr');

                var selectedOptionsArray = [];
                var minRank = Number.MAX_VALUE;
                ko.utils.arrayForEach(self.selectedOptions(), function (selection) {

                    var $optionElement = selectionRows.filter(function () {
                        return $(this).attr('data-id') == selection.Id;
                    });

                    var rank = $("#selectionTable > tbody > tr").index($optionElement[0]);
                    minRank = Math.min(minRank, rank);

                    selectedOptionsArray.push({
                        OptionId: selection.Id,
                        PollId: pollId,
                        PollValue: rank,
                        Token: { TokenGuid: token }
                    });
                });

                // Offset by the first value to account for table headers and sort out 0 index
                selectedOptionsArray.map(function (option) {
                    option.PollValue -= minRank - 1;
                });

                $.ajax({
                    type: 'PUT',
                    url: '/api/user/' + userId + '/vote',
                    contentType: 'application/json',
                    data: JSON.stringify(selectedOptionsArray),

                    success: function (returnData) {
                        $('#resultSection > div')[0].click();
                    }
                });


            }
        };

        self.getVotes = function (pollId, userId) {
            resetOptions();

            $.ajax({
                type: 'GET',
                url: '/api/user/' + userId + '/poll/' + pollId + '/vote',
                contentType: 'application/json',

                success: function (data) {
                    data.sort(sortByPollValue);
                    selectPickedOptions(data);
                }
            });
        };

        self.getResults = function (pollId) {

            $.ajax({
                type: 'GET',
                url: '/api/poll/' + pollId + '/vote',

                success: function (data) {
                    displayResults(data);
                }
            });
        };

        $(document).ready(function () {
            $(".sortable").sortable({
                items: 'tbody > tr',
                connectWith: '.sortable',
                axis: 'y',
                dropOnEmpty: true,
                receive: function (e, ui) {
                    var itemId = ui.item.attr('data-id');
                    var item = ko.utils.arrayFirst(self.options(), function (item) {
                        return item.Id == itemId;
                    });

                    var rows = $(this).find('tbody > tr');
                    var rowIds = $.map(rows, function (d, i) { return $(d).data("id") });
                    var insertionIndex = rowIds.indexOf(item.Id);

                    if ($(e.target).hasClass('selection-content')) {
                        //Insert at index in selectedOptions
                        self.selectedOptions.splice(insertionIndex, 0, item);
                    } else {
                        //Get index of where we dropped the item
                        self.selectedOptions.remove(item);
                    }

                    //Make sure rows are a part of the table body
                    if (rows.length == 0) {
                        $(this).find('tbody').append(ui.item)
                    }
                    else {
                        rows.eq(insertionIndex).before(ui.item);
                    }

                }
            });
        });

        $.ajax({
            type: 'GET',
            url: '/Partials/VotingStrategies/RankedVoteResults.html',
            dataType: 'html',

            success: function (data) {
                $("#results").append(data);
                ko.applyBindings(self, $('#results')[0]);
            }
        });
    }

});