require(['jquery', 'Common'], function ($, Common) {
    var pollId = Common.getPollId();
    var manageId = Common.getManageId();

    var pageToLoad;
    var javascriptToLoad;

    if (pollId) {
        // Go to voting
        pageToLoad = "/Partials/Poll.html"
        javascriptToLoad = '/Scripts/Poll.js';
    }
    else if (manageId) {
        // Go to poll management
        pageToLoad = "/Partials/Manage.html"
        javascriptToLoad = '/Scripts/Manage.js';
    }
    else {
        // Go to poll creation
        pageToLoad = "/Partials/Create.html"
        javascriptToLoad = '/Scripts/Create.js';
    }

    // Load partial HTML
    $.ajax({
        type: 'GET',
        url: pageToLoad,
        dataType: 'html',

        success: function (data) {
            require([javascriptToLoad]);
            $("#content").append(data);
        }
    })
});