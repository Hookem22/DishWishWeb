<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="DishWishWeb.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="Styles/Style.css" rel="stylesheet" />
    <script src="Scripts/jquery-2.0.3.min.js"></script>
    <script src="https://maps.googleapis.com/maps/api/js?v=3.exp&sensor=false"></script>

<script type="text/javascript">
    var currentLat = 30.3077609;
    var currentLng = -97.7534014;

    function SearchPlaces() {

        var searchTerm = $("#PlaceTextbox").val();
        if (!searchTerm) {
            $(".resultsDiv").hide();
            return;
        }

        $(".search .fbLoading").show();

        $.ajax({
            type: "POST",
            url: "Default.aspx/SearchPlaces",
            data: "{placeName:'" + escapeChars(searchTerm) + "', latitude:" + currentLat + ", longitude: " + currentLng + "}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                $(".search .fbLoading").hide();
                if (!$("#PlaceTextbox").val()) {
                    $(".resultsDiv").hide();
                    return;
                }
                PlacesList(data.d);
            }
        });
    }

    function PlacesList(results) {
        if (!results.length) {
            var prevLength = $(".search ul.results li");
            if (prevLength.length > 0) {
                return;
            }
        }

        var items = "";
        $(results).each(function (i) {
            var that = this;
            items += '<li id="' + i + 'li" onclick="AddPlace(' + i + ');" latitude="' + this.Latitude + '" longitude="' + this.Longitude + '" googleid="' + this.GoogleId + '" googlereferenceid="' + this.GoogleReferenceId + '" ><a>' + this.Name + '</a></li>';
        });

        if (!items)
            items += '<li><a>No Places Found</a></li>';

        $(".results").html(items);
        $(".resultsDiv").show();

    }

    function AddPlace(id) {

        var name = "";
        var city = $("#CityTextbox").val();

        if (id >= 0) {

            name = $("#" + id + "li a")[0].innerHTML;
            var latitude = $("#" + id + "li a").parent().attr("latitude");
            var longitude = $("#" + id + "li a").parent().attr("longitude");
            var googleid = $("#" + id + "li a").parent().attr("googleid");
            var googlereferenceid = $("#" + id + "li a").parent().attr("googlereferenceid");

            $("#PlaceTextbox").val(name);
            $("#LatitudeTextbox").val(latitude);
            $("#LongitudeTextbox").val(longitude);
            $("#GoogleIdTextbox").val(googleid);
            $("#GoogleReferenceIdTextbox").val(googlereferenceid);
        }
        else {
            name = $("#PlaceTextbox").val();
        }
        $.ajax({
            type: "POST",
            url: "Default.aspx/GoogleImages",
            data: "{placeName:'" + escapeChars(name) + "', city:'" + escapeChars(city) + "'}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                PopulateImages(data.d);
            }
        });

        $(".resultsDiv").hide();
    }

    function PopulateImages(results) {
        var images = "";
        $(results).each(function () {
            images += '<li><a><img src="' + this + '"/></a></li>';
        });

        $(".pickImages").html(images);

        $("#PopupOverlay").show();
        $("#Popup").show();
    }

    function CityAutoComplete() {
        var city = $("#CityTextbox").val();
        if (!city)
            return;

        $.ajax({
            type: "POST",
            url: "Default.aspx/CityAutoComplete",
            data: "{city:'" + escapeChars(city) + "'}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                var items = "";
                $(data.d).each(function () {
                    items += '<li onclick="$(\'#CityTextbox\').val(\'' + this + '\'); ChangeCity();" style="display:inherit;"><a>' + this + '</a></li>';
                });

                $(".results").html(items);
                $(".resultsDiv").show();
            }
        });
    }

    function ChangeCity() {
        $(".resultsDiv").hide();

        var address = $("#CityTextbox").val();
        var geocoder = new google.maps.Geocoder();
        geocoder.geocode({ 'address': address }, function (results, status) {
            if (status == google.maps.GeocoderStatus.OK) {
                currentLat = results[0].geometry.location.lat();
                currentLng = results[0].geometry.location.lng();
            }
        });
    }

    function escapeChars(val) {
        val = val.replace(/'/g, "\\'");
        return val;
    }

</script>
</head>
<body>
    <form id="form1" runat="server">
        <div id="PopupOverlay" style="display:none;" ></div>
        <div id="Popup" class="PlacePopup" style="display:none;">
            <ul class="pickImages">

            </ul>
            <a class="button" style="position:fixed; right:5%;margin-right:80px;">Done</a>
            <a class="button" style="position:fixed; right:5%;" onclick="$('#PopupOverlay').hide();$('#Popup').hide();">Cancel</a>
        </div>
        <div class="search" >
            <input type="text" class="field" id="PlaceTextbox" onkeyup="SearchPlaces();" PlaceHolder="Places"><a class="button" onclick="AddPlace(-1);">Search</a>
            <input id="CityTextbox" type="text" value="Austin" onkeyup="CityAutoComplete(event);" PlaceHolder="City"/>
            <input id="LatitudeTextbox" type="text" PlaceHolder="Latitude" />
            <input id="LongitudeTextbox" type="text" PlaceHolder="Longitude" />
            <input id="GoogleIdTextbox" type="text" PlaceHolder="GoogleId" />
            <input id="GoogleReferenceIdTextbox" type="text" PlaceHolder="GoogleReferenceId" />
            <div class="resultsDiv">
                <ul class="results">

                </ul>
            </div>
            <div class="fbLoading" style="display:none;">
                <img src="http://hk.centamap.com/gc/img/loading.gif" />
            </div>
        </div>

        <br />
        <div id="images" style="display:none";>
            <input id="Image1Sort" type="text" value="1" /><asp:Image ID="Image1" runat="server" Height="400px" />
            <input id="Image2Sort" type="text" value="2" /><asp:Image ID="Image2" runat="server" Height="400px" />
            <input id="Image3Sort" type="text" value="3" /><asp:Image ID="Image3" runat="server" Height="400px" />
            <input id="Image4Sort" type="text" value="4" /><asp:Image ID="Image4" runat="server" Height="400px" />
        </div>
    </div>
    </form>
</body>
</html>
