var url = 'http://178.77.69.80/api/';

var maxWordSize = 40;
var minWordSize = 15;
var uppercaseThreshold = 0.7;

var words;

function onResize()
{
    $("#bilder").height($("#articles").position().top + $("#articles").height() - $("#bilder").position().top + 10);
}

$(window).resize(function () {
    onResize();
});

$(function(){
    $(".artikelSection").hide();
    $(".verlaufSection").hide();
    $(".bilderSection").hide();

    $.getJSON(url + "words", function( data ) {
        console.log(url + "words");
        console.log(data);

        var max = data[0].Count;
        var min = data[data.length - 1].Count;

        words = data;

        var output = "";
        var index = 0;
        $.each( data, function(key, val ) {
            var proportionalSize = ((val.Count - min) / (max - min));
            var square = (proportionalSize + 1) * (proportionalSize + 1);

            output += "<a href='#' onclick='setWord(" + index + ", this);' class='word'><span style='font-size: " + ((proportionalSize * (maxWordSize - minWordSize)) + minWordSize) + "px; "
                + "line-height: " + (square < 1.3 ? square : 1.3) + ";"
                + (proportionalSize > uppercaseThreshold ? "text-transform: uppercase;" : "") + "' >"
                + val.Word + "</span></a> ";

            index++;
        });

        $("#words").html(output);
    });

    setInterval(function () { onResize(); }, 300);
});

function setWord(index, element)
{
    var word = words[index];
    console.log(word);

    $(".bilderSection").show('slide', { direction: 'left' }, 300);
    
    $(".word").removeClass("activeWord");
    $(element).toggleClass("activeWord");

    //$("#articles").hide();
    //$("#statistics").hide();

    //$("#leftContainer").removeClass("col-md-offset-3");

    //statistic
    $.getJSON(url + "words/statistic/" + word.Word, function( data ) {
        console.log(url + "words/statistic/" + word.Word);
        console.log(data);

        var xData = [];
        var yData = [];

        i = 0;
        $.each( data, function(key, val ) {
            xData.push(val.date.substr(0, 10));
            yData.push({ y: val.count, x: Date.parse(val.date) });
        });

        $(".verlaufSection").show('slide', { direction: 'left' }, 300);

        var ctx = document.getElementById("chartCanvas").getContext("2d");

        if (typeof theLineChart !== 'undefined') {
            theLineChart.destroy();
        }

        theLineChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: xData,
                datasets: [{
                    label: 'Artikel',
                    data: yData,
                    lineTension: 0.2,
                    borderColor: '#0000d2',
                    backgroundColor: 'rgba(0,0,0,0.1)'
                }],
                options: {
                    scales: {
                        xAxes: [{
                            type: 'linear',
                            position: 'bottom',
                        }]
                    }
                }
            },
            options: {
                legend: {
                    display: false
                }
            }
        });

        onResize();
    });

    //Articles
    $.getJSON(url + "articles/" + word.Word, function (data) {
        console.log(url + "articles/" + word.Word);
        console.log(data);

        count = 0;
        var output = "";
        $.each(data, function (key, val) {
            if (count < 400)
                output += "<a href='" + val.Url + "'>" + shortStr(val.Headline, 70) + "</a> <span class='sourceName'>[" + shortStr(val.Source.Name, 20) + "]</span><br />";

            count++;
        });

        $("#articles").html(output);
        $(".artikelSection").show('slide', { direction: 'right' }, 300);

        onResize();
    });

    var bilderHTML = "";
    $.each(word.imgUrls, function (key, val) {
        bilderHTML += "<a target='_blank' href='https://www.google.de/search?q=" + word.Word + "&safe=off&tbm=isch'><img src='" + val + "' /></a>";
    });
    $("#bilder").html(bilderHTML);
}

function shortStr(string, number)
{
    if(string.length > number)
        string = string.substr(0, number - 4) + " ...";

    return string;
}