define(['jquery', 'knockout', 'Navbar'], function ($, ko) {

    function Common() {

    }

    var tokenGuid = '';

    Common.resolveToken = function (pollId, uriTokenGuid, callbackFn) {
        tokenGuid = uriTokenGuid || localStorage[pollId];
        if(!tokenGuid){
            $.ajax({
                type: 'GET',
                url: '/api/poll/' + pollId + '/token',
                contentType: 'application/json',

                success: function (data) {
                    tokenGuid = data;
                    localStorage[pollId] = data;

                    if (callbackFn) callbackFn();
                }
            });
        } else if (callbackFn)
            callbackFn();
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