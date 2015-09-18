var map;
var oms;
var markers;

var leftMap;
var foundMap;
var shownPageId;
var userLat;
var userLong;
var userZoom;
                                                                                                                                                                                                                                                                                     
var shareHTML = "<div class='shbar'><a href='https://twitter.com/intent/tweet?text=Just%20found%20this%20%23happynote!&url=http%3A%2F%2Fhappynotez.org%2F{id}' class='twitter-share-button' data-url='http://happynotez.org/{id}' data-text='Just found this #happynote!'>Tweet</a> " +
                "<div class='fb-share-button' data-href='http://happynotez.org/{id}' data-layout='button_count'></div> <img class='r' src='img/flag.png' onclick='report({id})' title='Not a note?' /></div>";
                              
function withID(str, id)
{
    return str.split("{id}").join(id);
}

function switchToPage(id)
{
    var el = document.getElementById(id);
    if (el)
    {
        if (shownPageId == id)
        {
            shownPageId = null;
            el.style.display = "none";
        }
        else
        {
            switchToPage();

            el.style.display = "";
            shownPageId = id;
        }
    }
    else if (shownPageId)
    {
        var elShown = document.getElementById(shownPageId);
        if (elShown)
            elShown.style.display = "none";

        shownPageId = null;
    }

    return false;
}

function navFound()
{
    switchToPage("foundPage");

    if (!foundMap && google && google.maps && google.maps.Map)
        foundMap = initPicker("found");

    return false;
}

function navExplore()
{
    switchToPage("explorePage");

    if (shownPageId == "explorePage" && typeof(JSON) != "undefined" && typeof(XMLHttpRequest) != "undefined")
    {
        var page = document.getElementById("explorePage");
        if (page)
        {
            page.innerHTML = "Loading...";

            var http = new XMLHttpRequest();
            http.onreadystatechange = function ()
            {
                if (http.readyState == 4 && http.status == 200)
                {
                    var noteData = JSON.parse(http.responseText);
                    if (noteData && noteData.count)
                    {
                        page.innerHTML = "";

                        for (var i = 0; i < noteData.count; i++)
                        {
                            var node = document.createElement("DIV");
                            node.className = "ni";
                            node.innerHTML = withID("<img src='n/t{id}.jpg' onclick='showNote({id})' />" + shareHTML, noteData.ids[i]);
                            page.appendChild(node);
                        }
                    }
                    else
                    {
                        page.innerHTML = "Oh wow, you found the very first happy note! Congratulations!";
                    }

                    if (typeof(FB) != "undefined" && FB)
                        FB.XFBML.parse(page);
                    if (typeof(twttr) != "undefined" && twttr.widgets)
                        twttr.widgets.load(document.getElementById(page));
                }
            };
            http.open("GET", "/notez/coords", true);
            http.send();
        }
    }
}

function preview(src, target, label, m)
{
    var t = document.getElementById(target);
    var l = document.getElementById(label);
    if (t)
        if (src && src.files && src.files[0])
        {
            var reader = new FileReader();
            reader.onload = function ()
            {
                t.src = reader.result;
                l.style.display = "none";
                t.style.display = "";
            }

            reader.readAsDataURL(src.files[0]);
        }
        else
        {
            resetPreview(target, label);
        }
}
function resetPreview(target, label)
{
    var t = document.getElementById(target);
    var l = document.getElementById(label);
    if (t && t.style.display != "none")
    {
        t.style.display = "none";
        t.src = "//:0";
        l.style.display = "";
    }
}

function showMarker(m)
{
    if (m.info)
    {
        m.info.close();
        m.info = null;
    }
    else
    {
        var day = m.found;
        var year = Math.floor(day / 12 / 32); day = day - year * 12 * 32;
        var month = Math.floor(day / 32); day = day - month * 32;
        var found = "<div class='notedate'>Found on " + new Date(2015 + year, month - 1, day).toDateString();
        m.info = new google.maps.InfoWindow({ content: found + withID("</div><img class='notemark' src='n/t{id}.jpg' onclick='showNote({id})' />" + shareHTML, m.noteID) });

        google.maps.event.addListener(m.info, 'domready', function ()
        {
            if (typeof(FB) != "undefined" && FB.XFBML)
                FB.XFBML.parse(document.getElementById("map"));
            if (typeof(twttr) != "undefined" && twttr.widgets)
                twttr.widgets.load(document.getElementById("map"));
        });
        
        m.info.open(map, m);
    }
}

function removeMarker(m)
{
    if (m.info)
    {
        m.info.close();
        m.info = null;
    }
    if (m.map) m.setMap(null);
    if (oms) oms.removeMarker(m);
}

function showMarkerByHash()
{
    var openID = 0;
    if (window.location.hash)
        openID = parseInt(window.location.hash.substring(1, window.location.hash.length));

    showMarkerById(openID);
}
function showMarkerById(openID)
{
    var openMarker;

    if (openID && markers)
        for (var i = 0; i < markers.length; i++)
            if (markers[i].noteID == openID)
                openMarker = markers[i];

    if (openMarker)
    {
        if (!openMarker.info)
            showMarker(openMarker);

        map.panTo(openMarker.position);
    }
}

function showNote(id)
{
    var img = document.getElementById("noteImage");
    img.src = "img/smile.gif";
    switchToPage("notePage");
    img.src = "n/" + id + ".jpg";
}

function hideMarkers()
{
    if (markers)
        for (var i = 0; i < markers.length; i++)
            if (markers[i].info)
                showMarker(markers[i]);
}

function initMap()
{
    var mapDiv = document.getElementById("map");
    var allZoom = Math.max(1, Math.round(Math.log(mapDiv.clientWidth) / Math.log(2) - 8));
    
    map = new google.maps.Map(mapDiv, { center: new google.maps.LatLng(26, 0), zoom: allZoom, streetViewControl: false });

    if (typeof(OverlappingMarkerSpiderfier) == "undefined" && omsinit)
        omsinit();

    if (typeof(OverlappingMarkerSpiderfier) != "undefined")
    {
        oms = new OverlappingMarkerSpiderfier(map, { markersWontMove: true, markersWontHide: true, keepSpiderfied: true});
        oms.addListener('click', showMarker);
    }

    if (map.addListener)
        map.addListener("click", clearScreen);

    if (typeof(JSON) != "undefined" && typeof(XMLHttpRequest) != "undefined")
    {
        var http = new XMLHttpRequest();
        http.onreadystatechange = function ()
        {
            if (http.readyState == 4 && http.status == 200)
            {
                var openMarker;
                var openID = urlOpenID;
                if (window.location.hash)
                    openID = parseInt(window.location.hash.substring(1, window.location.hash.length));

                markers = [];
                var markerData = JSON.parse(http.responseText);
                if (markerData && markerData.count)
                {
                    for (var i = 0; i < markerData.count; i++)
                    {
                        var latLng = new google.maps.LatLng(markerData.lats[i], markerData.longs[i]);
                        var marker = new google.maps.Marker({ position: latLng, map: map });
                        marker.noteID = markerData.ids[i];
                        marker.found = markerData.founds[i];

                        markers.push(marker);

                        if (marker.noteID == openID)
                            openMarker = marker;

                        if (oms)
                            oms.addMarker(marker);
                    }
                }

                if (typeof(MarkerClusterer) != "undefined")
                    var cluster = new MarkerClusterer(map, markers);

                if (openMarker)
                    showMarker(openMarker);
            }
        };
        http.open("GET", "/notez/coords", true);
        http.send();
    }
}

function report(id)
{
    if (confirm("You are about to report this picture as not being a note."))
    {
        if (typeof(XMLHttpRequest) != "undefined")
        {
            var http = new XMLHttpRequest();
            http.onreadystatechange = function ()
            {
                if (http.readyState == 4 && http.status == 200)
                    alert(http.responseText);
            };

            http.open("GET", "/notez/report/" + id, true);
            http.send();
        }

        //if (markers)
        //    for (var i = 0; i < markers.length; i++)
        //        if (markers[i].noteID == id)
        //        {
        //            removeMarker(markers[i]);
        //            markers.splice(i, 1);
        //            break;
        //        }
    }
}

function clickOnEnter(e, btn)
{
    e = e || window.event;
    if (e.keyCode == 13)
    {
        document.getElementById(btn).click();
        return false;
    }
    return true;
}

function clearScreen()
{
    if (shownPageId)
        switchToPage();

    else
        hideMarkers();
}

function clearOnEsc(e)
{
    e = e || window.event;
    if (e.keyCode == 27)
    {
        clearScreen();
        return false;
    }
    return true;
}

function userLocated(pos)
{
    userLat = pos.coords.latitude;
    userLong = pos.coords.longitude;
    userZoom = 15;
}

if (window && window.addEventListener)
{
    window.addEventListener('keypress', clearOnEsc);
    window.addEventListener('hashchange', showMarkerByHash);
}

//if (navigator.geolocation)
//    navigator.geolocation.getCurrentPosition(userLocated, null, { enableHighAccuracy: true });