define('ResultChart', ['jquery', 'knockout', 'insight'], function ($, ko, insight) {
    var chartCounter = 0;
    function ResultChart($element) {
        chartCounter++;

        var chart;

        this.drawChart = function (data, options) {
            if (!data.length) return;

            var dataUnchanged = chart && data.length === chart.series().length;
            var barCount = 0;
            for (var n = 0; n < data.length; n++) {
                barCount += data[n].Data.length;
                var chartSeries = chart ? chart.series() : null;
                dataUnchanged = dataUnchanged &&
                    chart && chart.series().length > n &&
                    JSON.stringify(data[n].Data) == JSON.stringify(chart.series()[n].data.rawData());
            }
            //Exit early if data has not changed
            if (dataUnchanged)
                return;

            var defaultSettings = {
                legend: false,
                ordered: true,
                columns: false,
                voteTitle: 'Votes',
                optionTitle: ''
            };
            var settings = $.extend(defaultSettings, options);

            // Hack to fix insight's lack of data reloading
            $element.html('');

            // Fixed height for column chart, but scale to number of rows for bar charts
            var chartHeight = settings.columns ? 400 : Math.min(barCount * 50 + 100, 600);

            chart = new insight.Chart('', $element[0])
                .width($element.width())
                .height(chartHeight);

            var voteAxis = new insight.Axis(settings.voteTitle, insight.scales.linear);
            var optionAxis = new insight.Axis(settings.optionTitle, insight.scales.ordinal)
                    .isOrdered(settings.ordered);

            var xAxis = settings.columns ? optionAxis : voteAxis;
            var yAxis = settings.columns ? voteAxis : optionAxis;

            chart.xAxis(xAxis);
            chart.yAxis(yAxis);

            if (settings.legend) {
                var legendSeries = data.map(function (s, i) { return 'option-' + i; });
                chart.legend(new insight.Legend(legendSeries));
                chart.margin({ left: 85, right: 70, top: 0, bottom: 45 });
            } else {
                chart.autoMargin(true);
            }

            // Add a chart series for each data series
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

            // Add a chart series for the annotations
            if (settings.annotations) {
                settings.annotations.forEach(function (annotation, index) {
                    var annotationSeries = new insight.MarkerSeries('annotations',
                                new insight.DataSet(data[0].Data), xAxis, yAxis)
                        .keyFunction(function (d) {
                            return d.Name;
                        })
                        .valueFunction(function () { return annotation.value })
                        .tooltipFunction(function () { return annotation.tip })
                        .widthFactor(1.1)
                        .thickness(2);
                    allSeries.push(annotationSeries);
                });
            }

            chart.series(allSeries);

            // First parameter disables animation
            chart.draw(true);
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