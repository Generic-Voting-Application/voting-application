require.config({
    paths: {
        'jquery': 'Lib/jquery-2.1.1.min',
        'knockout': 'Lib/knockout-3.2.0',
        'bootstrap': 'Lib/bootstrap.min',
        'insight': 'Lib/insight.min',
        'd3': 'Lib/d3.min',
        'crossfilter': 'Lib/crossfilter.min',
        'sortable': 'Lib/jquery-sortable-min',
        'jqueryUI': 'Lib/jquery-ui.min',
        'jqueryTouch': 'Lib/jquery.ui.touch-punch.min'
},
    shim: {
        'jquery': {
            exports: '$'
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
        }
    }
});