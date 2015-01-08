require(['jquery', 'knockout', 'datetimepicker', 'moment', 'Common', 'jqueryUI'], function ($, ko, datetimepicker, moment, Common) {
    function HomeViewModel() {
        var self = this;

        self.templates = ko.observableArray();
        self.selectedStrategy = ko.observable();
        self.expires = ko.observable(false);

        self.pollName = ko.observable("");
        self.creatorName = ko.observable("");
        self.creatorEmail = ko.observable("");

        self.createPoll = function () {
            //Clear out previous error messages
            $('text').remove('.error-message');

            var inputs = $("#poll-create-form div");
            for (var i = 0; i < inputs.length; i++) {
                self.validateField(inputs[i]);
            }

            if (!$("#poll-create-form")[0].checkValidity()) {
                return;
            }

            $("#poll-create-btn").attr('disabled', 'disabled');
            $("#poll-create-btn").text('Creating...');

            var templateId = $("#template").val();
            var strategy = $("#voting-strategy").val();
            var maxPoints = $("#max-points").val() || 7;
            var maxPerVote = $("#max-per-vote").val() || 3;
            var inviteOnly = $('#invite-only').is(':checked');
            var anonymousVoting = $('#anonymous-voting').is(':checked');
            var requireAuth = $('#require-auth').is(':checked');
            var expiry = $('#expiry').is(':checked');
            var expiryDate = expiry ? new Date($('#expiry-date').val()) : null;
            var optionAdding = $('#option-adding').is(':checked');

            if (expiryDate == 'Invalid Date' && expiry) {
                expiryDate = new Date();
                expiryDate.setMinutes(expiryDate.getMinutes() + 30);
            }
            
            $.ajax({
                type: 'POST',
                url: '/api/poll',
                contentType: 'application/json',

                data: JSON.stringify({
                    Name: self.pollName(),
                    Creator: self.creatorName(),
                    Email: self.creatorEmail(),
                    templateId: templateId,
                    VotingStrategy: strategy,
                    MaxPoints: maxPoints,
                    MaxPerVote: maxPerVote,
                    InviteOnly: inviteOnly,
                    AnonymousVoting: anonymousVoting,
                    RequireAuth: requireAuth,
                    Expires: expiry,
                    ExpiryDate: expiryDate,
                    OptionAdding: optionAdding
                }),

                success: function (data) {
                    self.pollCreated(data.UUID, data.ManageID);
                }
            });
        };

        self.pollCreated = function (PollId, ManageId) {

            // Simulate a page change and make the back button simply refresh the page.
            history.pushState({}, "");
            window.addEventListener("popstate", function (e) {
                history.go(0);
            });

            // Load partial HTML
            $.ajax({
                type: 'GET',
                url: '/Partials/CheckEmail.html',
                dataType: 'html',

                success: function (data) {
                    $("#content").html(data);
                    $("#poll-id").attr("href", "/?poll=" + PollId);
                    $("#manage-id").attr("href", "/?manage=" + ManageId);
                }
            });
        };

        self.validateField = function (field) {
            var inputField = $(field).find('input')[0];

            if (!inputField) {
                return;
            }

            if (!inputField.checkValidity()) {
                $(inputField).addClass('error');
                var errorMessage = inputField.validationMessage;
                $(field).append('<text class="error-message">' + errorMessage + '</text>');
            }
            else {
                $(inputField).removeClass('error');
            }
        };

        self.populateTemplates = function () {
            $.ajax({
                type: 'GET',
                url: '/api/template',

                success: function (data) {
                    self.templates(data);
                }
            });
        };

        var setupTooltips = function() {
            $(document).tooltip

            function showOrHideElement(show, element) {
                element.next(".tip").toggle(show);
            }

            var tooltipTargets = $(".help-message");

            for (var i = 0; i < tooltipTargets.length; i++)
            {
                var $hoverTarget = $(tooltipTargets[i]);

                var hideElement = showOrHideElement.bind(null, false, $hoverTarget);
                var showElement = showOrHideElement.bind(null, true, $hoverTarget);
                ko.utils.registerEventHandler($hoverTarget, "mouseover", showElement);
                ko.utils.registerEventHandler($hoverTarget, "mouseout", hideElement);
                hideElement();
            }
        }

        $(document).ready(function () {
            self.populateTemplates();

            var defaultExpiryDate = moment().add(30, 'minutes');
            $('#expiry-date').datetimepicker({ defaultDate: defaultExpiryDate, minDate: moment()});

            // Select first tab
            $('#tabBar li a:first').tab('show')

            setupTooltips();
        });
    }

    ko.applyBindings(new HomeViewModel());
});
