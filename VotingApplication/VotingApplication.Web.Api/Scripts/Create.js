﻿require(['jquery', 'knockout', 'Common'], function ($, ko, Common) {
    function HomeViewModel() {
        var self = this;

        self.templates = ko.observableArray();

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

            var creatorName = $("#poll-creator").val();
            var pollName = $("#poll-name").val();
            var email = $("#email").val();
            var templateId = $("#template").val();
            var invites = $("#invites").val();
            var strategy = $("#voting-strategy").val();
            
            $.ajax({
                type: 'POST',
                url: '/api/poll',
                contentType: 'application/json',

                data: JSON.stringify({
                    Name: pollName,
                    Creator: creatorName,
                    Email: email,
                    Invites: invites.split('\n'),
                    templateId: templateId,
                    VotingStrategy: strategy
                }),

                success: function () {
                    self.pollCreated();
                }
            });
        };

        self.pollCreated = function () {
            // Load partial HTML
            $.ajax({
                type: 'GET',
                url: '/Partials/CheckEmail.html',
                dataType: 'html',

                success: function (data) {
                    $("#content").html(data);
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

        $(document).ready(function () {
            self.populateTemplates();
        });
    }

    ko.applyBindings(new HomeViewModel());
});