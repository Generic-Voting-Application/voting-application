define('ResultChart', ['jquery', 'knockout', 'insight'], function ($, ko, insight) {
    var chartCounter = 0;
    function ResultChart($element) {
        chartCounter++;

        var chart;

        this.drawChart = function (data, options) {
            if (!data.length) return;

            var dataUnchanged = false;
            var barCount = 0;
            for (var n = 0; n < data.length; n++) {
                barCount += data[n].Data.length;
                dataUnchanged = dataUnchanged &&
                    chart && chart.series().length > n &&
                    JSON.stringify(data[n].Data) == JSON.stringify(chart.series()[n].Data.rawData());
            }
            //Exit early if data has not changed
            if (dataUnchanged)
                return;

            var defaultSettings = {
                legend: false,
                ordered: true,
                columns: false
            };
            var settings = $.extend(defaultSettings, options);

            // Hack to fix insight's lack of data reloading
            $element.html('');

            chart = new insight.Chart('', $element[0])
                .width($element.width())
                .height(400); //Math.min(barCount * 50 + 100, 400));

            var voteAxis = new insight.Axis('Votes', insight.scales.linear);
            var optionAxis = new insight.Axis('', insight.scales.ordinal)
                    .isOrdered(settings.ordered);

            var xAxis = settings.columns ? optionAxis : voteAxis;
            var yAxis = settings.columns ? voteAxis : optionAxis;

            chart.xAxis(xAxis);
            chart.yAxis(yAxis);

            if (settings.legend) {
                chart.legend(new insight.Legend());
            }

            var insightSeries = settings.columns ? insight.ColumnSeries : insight.RowSeries;
            var allSeries = data.map(function (series, i) {
                return new insightSeries('option-' + i, new insight.DataSet(series.Data), xAxis, yAxis)
                    .keyFunction(function (d) {
                        return d.Name;
                    })
                    .valueFunction(function (d) {
                        return d.Sum;
                    })
                    .title(series.Name)
                    .tooltipFunction(function (d) {
                        var voterCount = d.Voters.length;
                        var votersDisplay = d.Voters;
                        var addition = "";

                        var maxToDisplay = 5;
                        if (voterCount > maxToDisplay) {
                            votersDisplay = d.Voters.slice(0, maxToDisplay);
                            addition = "<br />+ " + (voterCount - maxToDisplay) + " others";
                        }

                        return (series.Name ? series.Name + "<br />" : "")
                            + "Votes: " + d.Sum + "<br />" + votersDisplay.toString().replace(/,/g, "<br />") + addition;
                    });
            });

            chart.series(allSeries);
            chart.draw();
        };
    };

    ko.bindingHandlers.resultChart = {
        init: function (element) {
            var $element = $(element);
            $element.data('resultChart', new ResultChart($element));
        },
        update: function (element, valueAccessor, allBindingsAccessor) {
            var options = ko.unwrap(allBindingsAccessor().chartOptions);
            $(element).data('resultChart').drawChart(ko.unwrap(valueAccessor()), options);
        }
    };
});