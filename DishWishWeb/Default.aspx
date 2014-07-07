<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="DishWishWeb.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="Styles/Style.css" rel="stylesheet" />
    <script src="Scripts/jquery-2.0.3.min.js"></script>
    <script src="https://maps.googleapis.com/maps/api/js?v=3.exp&sensor=false"></script>

    <script src="http://oauth.googlecode.com/svn/code/javascript/oauth.js"></script>
    <script src="http://oauth.googlecode.com/svn/code/javascript/sha1.js"></script>

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

                        SetPlace(place);
                    }
                });
            }
            else
                SetPlace(place);
        }
        else {
            place = GetPlace();
        }

        $("#imagesDiv").html("");

        if (place.Id && id >= 0) {
            var container = "http://dishwishes.blob.core.windows.net/places/" + place.Id + "_";

            var urls = [];
            for(var i = 0; i < place.ImageCount; i++) {
                urls.push(container + i + ".jpg");
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
                $("#ImagePopup").show();

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
        $("#ImagePopup").hide();

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

                var list = "";
                var ct = $("#imagesDiv li").length;
                $(data.d).each(function (i) {
                    var j = ct + i;
                    list += '<li><input type="text" style="width:50px;" value="' + j + '" /><a onclick="DeleteImage(' + j + ')" style="margin-left:6px;" >Delete</a><br/><img src="' + this + '" style="width: ' + imgWidth + 'px;" /></li>';
                });

                $("#imagesDiv").append(list);
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

    function AddImageUrl() {
        var urls = [];
        $("#imagesDiv img").each(function () {
            urls.push($(this).attr("src"));
        });

        urls.push($("#ImageUrl").val());
        $("#ImageUrl").val("");

        $("#imagesDiv").html("");
        DownloadImages(urls);
    }

    function DeleteImage(id) {

        var urls = [];
        $("#imagesDiv img").each(function (i) {
            if(id != i)
                urls.push($(this).attr("src"));
        });

        $("#imagesDiv").html("");
        DownloadImages(urls);
    }

    //Menu Edit
    $(document).bind('click', function () {
        $('.menuArrow').bind('click', function () {
            var menuType = $(this).attr("title");
            var url = $("#" + menuType).val();
            if(url)
                window.open(url);
        });
    });

    function SavePlace()
    {
        $(".search .fbLoading").show();
        $(".resultsDiv").hide();

        var place = GetPlace();

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
            data: "{place:" + JSON.stringify(place) + ", sortOrder:" + JSON.stringify(sortOrder) + "}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {

                SetPlace(data.d);

                $("#Id").val(data.d.Id);

                var id = data.d.Id;
                var imageCount = data.d.ImageCount;

                $.ajax({
                    type: "POST",
                    url: "Default.aspx/GetImageSizes",
                    data: "{place:" + JSON.stringify(data.d) + "}",
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function (sizes) {
                        $(".search .fbLoading").hide();

                        var container = "http://dishwishes.blob.core.windows.net/places/" + id + "_";
                        var list = "<ul>";
                        var wd = imgWidth / 2;

                        for (var i = 0; i < imageCount; i++) {
                            list += '<li><img src="' + container + i + '.jpg" style="width: ' + wd + 'px;" /><br/>Size: ' + sizes.d[i] + '</li>';
                        }

                        list += '<li>'
                        for (var i = imageCount; i < sizes.d.length; i++)
                        {
                            list += "Menu size: " + sizes.d[i] + "<br/>";
                        }

                        list += "</ul>";
                        $("#imagesDiv").html(list);
                    }
                });
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

    function GetPlace()
    {
        var imageCount = $("#imagesDiv li input[type='text']").length;
        var place = {   
            Id: $("#Id").val(),
            Name: $("#Place").val(),
            GoogleId: $("#GoogleId").val(),
            GoogleReferenceId: $("#GoogleReferenceId").val(),
            YelpId: $("#YelpId").val(),
            ImageCount: imageCount,
            Latitude: $("#Latitude").val(),
            Longitude: $("#Longitude").val(),
            Website: $("#Website").val(),
            Menu: $("#Menu").val(),
            BrunchMenu: $("#BrunchMenu").val(),
            LunchMenu: $("#LunchMenu").val(),
            DrinkMenu: $("#DrinkMenu").val(),
            HappyHourMenu: $("#HappyHourMenu").val()
        };
        return place;
    }

    function SetPlace(place)
    {
        $("#Id").val(place.Id);
        $("#Place").val(place.Name);
        $("#GoogleId").val(place.GoogleId);
        $("#GoogleReferenceId").val(place.GoogleReferenceId);
        $("#YelpId").val(place.YelpId);
        $("#Latitude").val(place.Latitude);
        $("#Longitude").val(place.Longitude);
        $("#Website").val(place.Website);
        $("#WebsiteLink").attr("href", place.Website);
        $("#Menu").val(place.Menu);
        $("#BrunchMenu").val(place.BrunchMenu);
        $("#LunchMenu").val(place.LunchMenu);
        $("#DrinkMenu").val(place.DrinkMenu);
        $("#HappyHourMenu").val(place.HappyHourMenu);

        PlotMap(place.Latitude, place.Longitude);
    }

    function PlotMap(lat, lng) {
        var myLatLng = new google.maps.LatLng(lat, lng);

        var mapOptions = {
            zoom: 15,
            center: new google.maps.LatLng(lat, lng),
            mapTypeId: google.maps.MapTypeId.ROADMAP
        }

        map = new google.maps.Map(document.getElementById('map-canvas'), mapOptions);

        var marker = new google.maps.Marker({
            position: myLatLng,
            map: map
        });

        $("#map-canvas").css("overflow", "inherit");
    }

    $(document).keyup(function (e) {
        if (e.keyCode == 27) { $('#PopupOverlay').hide(); $('#ImagePopup').hide(); $('#MenuPopup').hide(); }   // esc
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
        <div id="ImagePopup" class="Popup" style="display:none;">
            <ul class="pickImages">

            </ul>
            <a class="button" style="position:fixed; right:5%;margin-right:80px;" onclick="AddImages();">Done</a>
            <a class="button" style="position:fixed; right:5%;" onclick="$('#PopupOverlay').hide();$('#ImagePopup').hide();">Cancel</a>
        </div>
        <div class="search" >
            <input type="text" class="field" id="Place" onkeyup="SearchPlaces();" PlaceHolder="Place">
            <a class="button" onclick="AddPlace(-1);">Search</a>
            <a class="button" onclick="SavePlace();">Save</a><br />
            <input id="City" type="text" value="Austin" onkeyup="CityAutoComplete(event);" PlaceHolder="City"/><br />
            <input id="Latitude" type="text" PlaceHolder="Latitude" /><br />
            <input id="Longitude" type="text" PlaceHolder="Longitude" /><br />
            <input id="GoogleId" type="text" PlaceHolder="GoogleId" /><br />
            <input id="GoogleReferenceId" type="text" PlaceHolder="GoogleReferenceId" /><br />
            <input id="YelpId" type="text" PlaceHolder="YelpId" /><br />
            <input id="Website" type="text" PlaceHolder="Website" /><a id="WebsiteLink" target="_blank" class="arrowLink"></a><br />
            <div id="map-canvas" style="left: 500px;top: 10px;height: 180px;width: 300px;position:absolute;"></div>
            <input id="Menu" type="text" style="width:200px;" PlaceHolder="Menu" /><a class="arrowLink menuArrow" title="Menu"></a>
            <input id="LunchMenu" type="text" style="width:200px;" PlaceHolder="Lunch Menu" /><a class="arrowLink menuArrow" title="LunchMenu"></a>
            <input id="BrunchMenu" type="text" style="width:200px;" PlaceHolder="Brunch Menu" /><a class="arrowLink menuArrow" title="BrunchMenu"></a>
            <input id="DrinkMenu" type="text" style="width:200px;" PlaceHolder="Drink Menu" /><a class="arrowLink menuArrow" title="DrinkMenu"></a>
            <input id="HappyHourMenu" type="text" style="width:200px;" PlaceHolder="Happy Hour Menu" /><a class="arrowLink menuArrow" title="HappyHourMenu"></a>
            <br />
            <input id="Id" type="text" PlaceHolder="PlaceId" />
            <br /><br />
            <input type="text" id="ImageUrl" PlaceHolder="Image Url">
            <a class="button" onclick="AddImageUrl();">Add Image</a>
            <div class="resultsDiv">
                <ul class="results">

                </ul>
            </div>
            <div class="fbLoading" style="display:none;">
                <img src="http://hk.centamap.com/gc/img/loading.gif" />
            </div>
        </div>

        <br />
        <div>
            <ul id="imagesDiv">

            </ul>
        </div>
    </div>
    </form>
</body>
</html>
