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

            var creatorName = $("#poll-creator").val();
            var pollName = $("#poll-name").val();
            var email = $("#email").val();
            var templateId = $("#template").val();

            $.ajax({
                type: 'POST',
                url: '/api/session',
                contentType: 'application/json',

                data: JSON.stringify({
                    Name: pollName,
                    Creator: creatorName,
                    optionSetId: templateId
                }),

                success: function (data) {
                    self.sendEmail(email, data);
                }
            });
        };

        self.sendEmail = function (email, data) {
            $.ajax({
                type: 'POST',
                url: '/api/mail',
                contentType: 'application/json',

                data: JSON.stringify({
                    email: email,
                    PollId: data,
                    ManageId: "123"
                })
            });
        }

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
        }

        self.allTemplates = function () {
            $.ajax({
                type: 'GET',
                url: '/api/optionset',

                success: function (data) {
                    self.templates(data);
                }
            })
        }
        
        $(document).ready(function () {
            self.allTemplates();
        });
    }

    ko.applyBindings(new HomeViewModel());
});
