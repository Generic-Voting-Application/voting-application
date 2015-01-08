require.config({
    baseUrl: '/Scripts',
    paths: {
        'jquery': 'Lib/jquery-2.1.1.min',
        'knockout': 'Lib/knockout-3.2.0',
        'bootstrap': 'Lib/bootstrap.min',
        'insight': 'Lib/insight.min',
        'd3': 'Lib/d3.min',
        'crossfilter': 'Lib/crossfilter.min',
        'sortable': 'Lib/jquery-sortable-min',
        'jqueryUI': 'Lib/jquery-ui.min',
        'jqueryTouch': 'Lib/jquery.ui.touch-punch.min',
        'platform': 'https://apis.google.com/js/client:platform',
        'moment': 'Lib/moment',
        'datetimepicker': 'Lib/bootstrap-datetimepicker.min',
        'countdown': 'Lib/countdown.min'
    },
    shim: {
        'jquery': {
            exports: '$'
        },
        'bootstrap': {
            deps: ['jquery']
        },
        'knockout': {
            deps: ['jquery'],
            exports: 'ko'
        },
        'insight': {
            deps: ['d3', 'crossfilter'],
            exports: 'insight'
        },
        'sortable': {
            deps: ['jquery']
        },
        'jqueryUI': {
            deps: ['jquery']
        },
        'jqueryTouch': {
            deps: ['jquery']
        },
        'datetimepicker': {
            deps: ['bootstrap', 'moment']
        }
    }
});

if (window._gvaRequireFile) require([window._gvaRequireFile]);
