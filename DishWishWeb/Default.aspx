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
    var imgWidth = 450;
    var cropping = false;

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
            if (this.Id)
                items += '<li id="' + i + 'li" onclick="AddPlace(' + i + ');" placeid="' + this.Id + '" imagecount=' + this.ImageCount + ' latitude="' + this.Latitude + '" longitude="' + this.Longitude + '" googleid="' + this.GoogleId + '" googlereferenceid="' + this.GoogleReferenceId + '" ><a style="font-weight: bold;font-size:14px;" >' + this.Name + '</a></li>';
            else
                items += '<li id="' + i + 'li" onclick="AddPlace(' + i + ');" placeid="" latitude="' + this.Latitude + '" longitude="' + this.Longitude + '" googleid="' + this.GoogleId + '" googlereferenceid="' + this.GoogleReferenceId + '" ><a>' + this.Name + '</a></li>';
        });

        if (!items)
            items += '<li><a>No Places Found</a></li>';

        $(".results").html(items);
        $(".resultsDiv").show();

    }

    function AddPlace(id) {

        var name = "";
        var city = $("#CityTextbox").val();
        if (city.indexOf(",") > 0)
            city = city.substr(0, city.indexOf(","));

        if (id >= 0) {

            name = $("#" + id + "li a")[0].innerHTML;
            var latitude = $("#" + id + "li a").parent().attr("latitude");
            var longitude = $("#" + id + "li a").parent().attr("longitude");
            var googleid = $("#" + id + "li a").parent().attr("googleid");
            var googlereferenceid = $("#" + id + "li a").parent().attr("googlereferenceid");
            var placeid = $("#" + id + "li a").parent().attr("placeid");

            $("#PlaceTextbox").val(name);
            $("#LatitudeTextbox").val(latitude);
            $("#LongitudeTextbox").val(longitude);
            $("#GoogleIdTextbox").val(googleid);
            $("#GoogleReferenceIdTextbox").val(googlereferenceid);
            $("#PlaceIdTextbox").val(placeid);
        }
        else {
            name = $("#PlaceTextbox").val();
        }

        if (placeid) {
            var ct = $("#" + id + "li a").parent().attr("imagecount");
            var container = "http://dishwishes.blob.core.windows.net/places/" + placeid + "_";

            var urls = [];
            for(var i = 0; i < ct; i++) {
                urls.push(container + i + ".png");
            }

            DownloadImages(urls);
        }
        else {
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
        }
        $(".resultsDiv").hide();
    }

    function PopulateImages(results) {
        var images = "";
        $(results).each(function () {
            images += '<li><div><img src="' + this + '" onclick="togglePickedImage(this)"/></div></li>';
        });

        $(".pickImages").html(images);

        $("#PopupOverlay").show();
        $("#Popup").show();
    }

    function togglePickedImage(img) {
        $(img).toggleClass("picked");
    }

    function AddImages() {

        $("#PopupOverlay").hide();
        $("#Popup").hide();


        var urls = [];
        $(".picked").each(function () {
            urls.push($(this).attr("src"));
        });

        DownloadImages(urls);
    }

    function DownloadImages(urls)
    {
        $.ajax({
            type: "POST",
            url: "Default.aspx/DownloadImages",
            data: "{urls:" + JSON.stringify(urls) + "}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {

                var list = "<ul>";
                $(data.d).each(function (i) {
                    list += '<li><input type="text" style="width:50px;" value="' + i + '" /><br/><img src="' + this + '" style="width: ' + imgWidth + 'px;" /></li>';
                });
                list += "</ul>";

                $("#imagesDiv").html(list);
            }
        });
    }

    //Crop
    $(document).bind('click', function () {
        $('#imagesDiv li img').bind('click', function (e) {
            if (cropping)
                return;

            cropping = true;

            var offset = $(this).offset();
            var x = (e.pageX - offset.left);
            var y = (e.pageY - offset.top);

            var percentCrop = x / imgWidth;

            var src = $(this).attr("src");
            var id = src.substr(src.lastIndexOf("_") + 1);
            if (id.indexOf(".png") >= 0)
                id = id.substr(0, id.indexOf(".png"));

            $(this).attr("src", "http://hk.centamap.com/gc/img/loading.gif");

            $.ajax({
                type: "POST",
                url: "Default.aspx/CropImage",
                data: "{id:'" + id + "', percentCrop:" + percentCrop + "}",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (data) {
                    $("#imagesDiv img:eq(" + id + ")").attr("src", src);// + "?" + new Date().getTime());
                    cropping = false;
                }
            });
        });
    });

    function SavePlace()
    {
        $(".search .fbLoading").show();
        $(".resultsDiv").hide();

        var name = $("#PlaceTextbox").val();
        var latitude = $("#LatitudeTextbox").val();
        var longitude = $("#LongitudeTextbox").val();
        var googleId = $("#GoogleIdTextbox").val();
        var googleReferenceId = $("#GoogleReferenceIdTextbox").val();
        var placeId = $("#PlaceIdTextbox").val();

        var place = "{ Id:'" + placeId + "', Name:'" + escapeChars(name) + "', Latitude:'" + latitude + "', Longitude:'" + longitude + "', GoogleId:'" + googleId + "', GoogleReferenceId:'" + googleReferenceId + "'}"
        var sortOrder = [];
        $("#imagesDiv li input[type='text']").each(function () {
            sortOrder.push($(this).val());
        });

        //Validation
        var sorted = [];
        $(sortOrder).each(function () {
            sorted.push(this);
        });

        sorted.sort();
        for (var i = 0; i < sorted.length; i++) {
            if (sorted[i] != i) {
                $(".results").html("<li><a>Sort Order Error</a></li>");
                $(".resultsDiv").show();
                $(".search .fbLoading").hide();
                return;
            }
        }


        $.ajax({
            type: "POST",
            url: "Default.aspx/SavePlace",
            data: "{place:" + place + ", sortOrder:" + JSON.stringify(sortOrder) + "}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                $(".search .fbLoading").hide();

                $("#PlaceIdTextbox").val(data.d.Id);

                var container = "http://dishwishes.blob.core.windows.net/places/" + data.d.Id + "_";
                var list = "<ul>";
                for(var i = 0, ii = data.d.ImageCount; i < ii; i++) {
                    list += '<li><input type="text" style="width:50px;" value="' + i + '" /><br/><img src="' + container + i + '.png" style="width: ' + imgWidth + 'px;" /></li>';                  
                }
                list += "</ul>";
                $("#imagesDiv").html(list);
            }
        });
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
            <a class="button" style="position:fixed; right:5%;margin-right:80px;" onclick="AddImages();">Done</a>
            <a class="button" style="position:fixed; right:5%;" onclick="$('#PopupOverlay').hide();$('#Popup').hide();">Cancel</a>
        </div>
        <div class="search" >
            <input type="text" class="field" id="PlaceTextbox" onkeyup="SearchPlaces();" PlaceHolder="Places">
            <a class="button" onclick="AddPlace(-1);">Search</a>
            <a class="button" onclick="SavePlace();">Save</a>
            <input id="CityTextbox" type="text" value="Austin" onkeyup="CityAutoComplete(event);" PlaceHolder="City"/>
            <input id="LatitudeTextbox" type="text" PlaceHolder="Latitude" />
            <input id="LongitudeTextbox" type="text" PlaceHolder="Longitude" />
            <input id="GoogleIdTextbox" type="text" PlaceHolder="GoogleId" />
            <input id="GoogleReferenceIdTextbox" type="text" PlaceHolder="GoogleReferenceId" />
            <input id="PlaceIdTextbox" type="text" PlaceHolder="PlaceId" />
            <div class="resultsDiv">
                <ul class="results">

                </ul>
            </div>
            <div class="fbLoading" style="display:none;">
                <img src="http://hk.centamap.com/gc/img/loading.gif" />
            </div>
        </div>

        <br />
        <div id="imagesDiv" >

        </div>
    </div>
    </form>
</body>
</html>
