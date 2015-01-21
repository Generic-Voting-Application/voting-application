define('ResultChart', ['jquery', 'knockout', 'insight'], function ($, ko, insight) {
    var chartCounter = 0;
    function ResultChart($element) {
        chartCounter++;

        var chart;

        this.drawChart = function (data) {
            //Exit early if data has not changed
            if (chart && JSON.stringify(data) == JSON.stringify(chart.series()[0].data.rawData()))
                return;

            // Hack to fix insight's lack of data reloading
            $element.html('');
            var voteData = new insight.DataSet(data);

            chart = new insight.Chart('', $element[0])
                .width($element.width())
                .height(data.length * 50 + 100);

            var xAxis = new insight.Axis('Votes', insight.scales.linear);
            var yAxis = new insight.Axis('', insight.scales.ordinal)
                .isOrdered(true);
            chart.xAxis(xAxis);
            chart.yAxis(yAxis);

            var series = new insight.RowSeries('votes', voteData, xAxis, yAxis)
            .keyFunction(function (d) {
                return d.Name;
            })
            .valueFunction(function (d) {
                return d.Sum;
            })
            .tooltipFunction(function (d) {
                var voterCount = d.Voters.length;
                var maxToDisplay = 5;
                if (voterCount <= maxToDisplay) {
                    return "Votes: " + d.Sum + "<br />" + d.Voters.toString().replace(/,/g, "<br />");
                }
                else {
                    return "Votes: " + d.Sum + "<br />" + d.Voters.slice(0, maxToDisplay).toString().replace(/,/g, "<br />") + "<br />" + "+ " + (voterCount - maxToDisplay) + " others";
                }
            });

            chart.series([series]);
            chart.draw();
        };
    };

    ko.bindingHandlers.resultChart = {
        init: function (element) {
            var $element = $(element);
            $element.data('resultChart', new ResultChart($element));
        },
        update: function (element, valueAccessor) {
            $(element).data('resultChart').drawChart(ko.unwrap(valueAccessor()));
        }
    };
});