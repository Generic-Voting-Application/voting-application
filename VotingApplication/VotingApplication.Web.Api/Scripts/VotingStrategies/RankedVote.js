define(['jquery', 'knockout', 'jqueryUI', 'Common'], function ($, ko, jqueryUI, Common) {

    return function RankedVote() {

        self = this;
        self.options = ko.observableArray();
        self.previousOptions = ko.observableArray();
        self.selectedOptions = ko.observableArray();

        var selectOption = function (option) {
            var $option = $('#optionTable > tbody > tr').filter(function () {
                return $(this).attr('data-id') == option.Id;
            });

            $('#selectionTable > tbody').append($option.remove());
            self.selectedOptions.push(option)
        }

        var selectPickedOptions = function (pickedOptions) {
            self.selectedOptions.removeAll();
            self.previousOptions.removeAll();

            pickedOptions.forEach(function (option) {
                var pickedOption = ko.utils.arrayFirst(self.options(), function (item) {
                    return item.Id == option.OptionId;
                });

                selectOption(pickedOption);
            });
        }

        self.doVote = function (data, event) {
            var userId = Common.currentUserId();
            var pollId = Common.getPollId();

            if (userId && pollId) {

                var selectionRows = $('#selectionTable > tbody > tr');

                var selectedOptionsArray = [];
                ko.utils.arrayForEach(self.selectedOptions(), function (selection) {

                    var $optionElement = selectionRows.filter(function () {
                        return $(this).attr('data-id') == selection.Id;
                    });

                    var rank = $optionElement[0].rowIndex;

                    selectedOptionsArray.push({
                        OptionId: selection.Id,
                        SessionId: pollId,
                        PollValue: rank
                    });
                });

                // Offset by the first value to account for table headers and sort out 0 index
                var lowestValue = selectedOptionsArray[0].PollValue - 1;
                selectedOptionsArray.map(function (option) {
                    option.PollValue -= lowestValue;
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
            $.ajax({
                type: 'GET',
                url: '/api/user/' + userId + '/session/' + pollId + '/vote',
                contentType: 'application/json',

                success: function (data) {
                    selectPickedOptions(data);
                }
            });
        };

        self.getResults = function (pollId) {
            $.ajax({
                type: 'GET',
                url: '/api/session/' + pollId + '/vote',

                success: function (data) {
                    // Do stuff with results here
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

                    if ($(e.target).hasClass('selection-content')) {
                        self.selectedOptions.push(item);
                    } else {
                        self.selectedOptions.remove(item);
                    }

                    $(this).find("tbody").append(ui.item);
                }
            });
        });
    }

});