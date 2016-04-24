var url = 'http://178.77.69.80/api/';

var maxWordSize = 60;
var minWordSize = 10;
var uppercaseThreshold = 0.7;

$(function(){
    $("#articles").hide();

    $.getJSON(url + "words", function( data ) {
        console.log(data);

        var max = data[0].Count;
        var min = data[data.length - 1].Count;

        var output = "";
        $.each( data, function(key, val ) {
            var proportionalSize = ((val.Count - min) / (max - min));
            var square = (proportionalSize + 1) * (proportionalSize + 1);

            output += "<a href='#' onclick='setWord(\""+val.Word+"\");'><span style='font-size: " + ((proportionalSize * (maxWordSize - minWordSize)) + minWordSize) +"px; "
                + "line-height: " + (square < 1.3 ? square : 1.3) + ";"
                + (proportionalSize > uppercaseThreshold ? "text-transform: uppercase;" : "") + "' >"
                + val.Word + "</span></a> ";
        });

        $("#words").html(output);
    });
});

function setWord(word)
{
    $.getJSON(url + "articles/" + word, function( data ) {
        console.log(data);

        var output = "";
        $.each( data, function(key, val ) {
            output += "<a href='"+val.Url+"'>"+shortStr(val.Headline)+"</a> <span class='sourceName'>["+val.Source.Name+"]</span><br />";
        });

        $("#articles").html(output);
        $("#articles").show();
    });
}

function shortStr(string)
{
    if(string.length > 80)
        string = string.substr(0, 76) + " ...";

    return string;
}