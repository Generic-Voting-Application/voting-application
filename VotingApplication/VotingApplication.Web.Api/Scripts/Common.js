define(['jquery', 'knockout', 'Navbar'], function ($, ko) {

    // Checks if elements are collapsed
    (function () {

        $.fn.isCollapsed = function () {
            var $this = $(this).find('.accordion-body').filter(':visible');
            return $this ? !$this.parents().toArray().some(function (element) {
                return !$(element).hasClass('in');
            }) : false;
        };

        $.fn.collapseSection = function (collapse) {
            if (($(this).isCollapsed() && collapse == 'hide') || (!$(this).isCollapsed() && collapse == 'show')) {
                return;
            }

            $(this).find('.accordion-body').collapse(collapse);
        };
    })();

    function Common() {

    }

    var tokenGuid = '';

    Common.resolveToken = function (pollId, uriTokenGuid) {
        tokenGuid = uriTokenGuid || localStorage[pollId];
        if(!tokenGuid){
            $.ajax({
                type: 'GET',
                url: '/api/poll/' + pollId + '/token',
                contentType: 'application/json',

                success: function (data) {
                    tokenGuid = data;
                    localStorage[pollId] = data;
                }
            });
        }
    }

    Common.getToken = function (pollId) {
        return localStorage[pollId];
    }

    Common.getVoterName = function (pollId) {
        return localStorage['userName'];
    };

    Common.setVoterName = function (userName, pollId) {
        localStorage['userName'] = userName;
    };    

    Common.clearStorage = function (pollId) {
        localStorage.removeItem(pollId);
        localStorage.removeItem('userName');
    }

    Common.handleError = function (error) {

        var friendlyText;

        switch (error.status) {
            case 404:
                friendlyText = 'Double check the poll ID and ensure that you have logged in correctly'
                break;
        }

        var newError = '<div class="alert alert-danger">' +
        '<a href="#" class="close" data-dismiss="alert">&times;</a>' +
        '<strong>' + error.statusText + ' </strong>' +
        JSON.parse(error.responseText).Message +
        (friendlyText ? '<br/>' + friendlyText : '') +
        '</div>';

        $('#errorArea').append(newError);
    }

    return Common;
});