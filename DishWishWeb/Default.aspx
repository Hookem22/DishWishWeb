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
    var places = [];

    function SearchPlaces() {

        var searchTerm = $("#Place").val();
        var city = $("#City").val();
        if (!searchTerm || searchTerm.length < 3) {
            $(".resultsDiv").hide();
            return;
        }

        $(".search .fbLoading").show();

        $.ajax({
            type: "POST",
            url: "Default.aspx/SearchPlaces",
            data: "{placeName:'" + escapeChars(searchTerm) + "',city:'" + escapeChars(city) + "', latitude:" + currentLat + ", longitude: " + currentLng + "}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                $(".search .fbLoading").hide();
                if (!$("#Place").val()) {
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
        places = [];
        $(results).each(function (i) {
            var style = "";
            if (this.Id)
                style = "font-weight:bold;font-size:12px;";
            else if (this.GoogleId && this.YelpId)
                style = "font-weight:bold;color:green;";
            else if (this.YelpId)
                style = "color:#c41200;";
            else
                style = "color:#4285f4";

            places.push(this);
            items += '<li onclick="AddPlace(' + i + ');" ><a style="' + style + '" >' + this.Name + '</a></li>';

        });

        if (!items)
            items += '<li><a>No Places Found</a></li>';

        $(".results").html(items);
        $(".resultsDiv").show();

    }

    function AddPlace(id) {

        var place = places[id];
        console.log(place);
        var city = $("#City").val();

        if (id >= 0) {

            if (!place.Latitude) {
                var address = place.GoogleReferenceId;
                var geocoder = new google.maps.Geocoder();
                geocoder.geocode({ 'address': address }, function (results, status) {
                    if (status == google.maps.GeocoderStatus.OK) {
                        place.Latitude = results[0].geometry.location.lat();
                        place.Longitude = results[0].geometry.location.lng();
                        place.GoogleReferenceId = "";

                        fillTextboxes(place);
                    }
                });
            }
            else
                fillTextboxes(place);
        }
        else {
            place = {   GoogleId: $("#GoogleId").val(),
                GoogleReferenceId: $("#GoogleReferenceId").val(),
                Id: $("#PlaceId").val(),
                ImageCount: 0,
                Latitude: $("#Latitude").val(),
                Longitude: $("#Longitude").val(),
                Name: $("#Place").val(),
                Website: $("#Website").val(),
                YelpId: $("#YelpId").val(),
            };
        }

        if (place.Id) {
            var container = "http://dishwishes.blob.core.windows.net/places/" + place.Id + "_";

            var urls = [];
            for(var i = 0; i < place.ImageCount; i++) {
                urls.push(container + i + ".png");
            }

            DownloadImages(urls);
        }
        else {
            $(".pickImages").html("");

            $("#Website").val("");
            $("#WebsiteLink").attr("href", "");
            if (place.Website) {
                $.ajax({
                    type: "POST",
                    url: "Default.aspx/GetWebsite",
                    data: "{yelpUrl:'" + place.Website + "'}",
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function (data) {
                        $("#Website").val(data.d);
                        $("#WebsiteLink").attr("href", data.d);

                        if (data.d) {
                            GetWebsiteImages(place.Name, city, data.d);
                        }
                    }
                });
            }
            else
            {
                GoogleImages(place.Name, city)
            }
        }
        $(".resultsDiv").hide();
    }

    function fillTextboxes(place)
    {
        $("#Place").val(place.Name);
        $("#Latitude").val(place.Latitude);
        $("#Longitude").val(place.Longitude);
        if (place.GoogleId)
            $("#GoogleId").val(place.GoogleId);
        if (place.GoogleReferenceId)
            $("#GoogleReferenceId").val(place.GoogleReferenceId);
        if (place.YelpId)
            $("#YelpId").val(place.YelpId);
        if (place.PlaceId)
            $("#PlaceId").val(place.PlaceId);
    }

    function GoogleImages(name, city) {
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

    function GetWebsiteImages(name, city, url) {
        $.ajax({
            type: "POST",
            url: "Default.aspx/GetWebsiteImages",
            data: "{url:'" + url + "'}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                var images = "";
                $(data.d).each(function () {
                    images += '<li><div><img src="' + this + '" onclick="togglePickedImage(this)"/></div></li>';
                });

                $(".pickImages").append(images);

                $("#PopupOverlay").show();
                $("#Popup").show();

                GoogleImages(name, city);
            }
        });
    }

    function PopulateImages(results) {
        var images = "";
        $(results).each(function () {
            images += '<li><div><img src="' + this + '" onclick="togglePickedImage(this)"/></div></li>';
        });

        $(".pickImages").append(images);

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

        var name = $("#Place").val();
        var latitude = $("#Latitude").val();
        var longitude = $("#Longitude").val();
        var googleId = $("#GoogleId").val();
        var googleReferenceId = $("#GoogleReferenceId").val();
        var placeId = $("#PlaceId").val();

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

                $("#PlaceId").val(data.d.Id);

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
        var city = $("#City").val();
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
                    items += '<li onclick="$(\'#City\').val(\'' + this + '\'); ChangeCity();" style="display:inherit;"><a>' + this + '</a></li>';
                });

                $(".results").html(items);
                $(".resultsDiv").show();
            }
        });
    }

    function ChangeCity() {
        $(".resultsDiv").hide();

        var address = $("#City").val();
        var geocoder = new google.maps.Geocoder();
        geocoder.geocode({ 'address': address }, function (results, status) {
            if (status == google.maps.GeocoderStatus.OK) {
                currentLat = results[0].geometry.location.lat();
                currentLng = results[0].geometry.location.lng();

                if (address.indexOf(",") > 0)
                    $("#City").val(address.substr(0, address.indexOf(",")));
            }
        });
    }

    $(document).keyup(function (e) {
        if (e.keyCode == 27) { $('#PopupOverlay').hide(); $('#Popup').hide(); }   // esc
    });

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
            <input type="text" class="field" id="Place" onkeyup="SearchPlaces();" PlaceHolder="Place">
            <a class="button" onclick="AddPlace(-1);">Search</a>
            <a class="button" onclick="SavePlace();">Save</a>
            <input id="City" type="text" value="Austin" onkeyup="CityAutoComplete(event);" PlaceHolder="City"/>
            <input id="Latitude" type="text" PlaceHolder="Latitude" />
            <input id="Longitude" type="text" PlaceHolder="Longitude" />
            <input id="GoogleId" type="text" PlaceHolder="GoogleId" />
            <input id="GoogleReferenceId" type="text" PlaceHolder="GoogleReferenceId" />
            <input id="YelpId" type="text" PlaceHolder="YelpId" />
            <input id="Website" type="text" PlaceHolder="Website" /><a id="WebsiteLink" target="_blank"><img style="height: 22px;vertical-align: -6px;" src="http://www.artdocks.com/wp-content/uploads/2013/07/iconmonstr-arrow-28-icon.png" /></a>
            <input id="Menu" type="text" PlaceHolder="Menu" />
            <input id="PlaceId" type="text" PlaceHolder="PlaceId" />
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
