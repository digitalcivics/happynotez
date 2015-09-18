/**
 *
 * A NON-JQUERY GOOGLE MAPS LATITUDE AND LONGITUDE LOCATION PICKER
 * version 1.2
 *
 * Supports multiple maps. Works on touchscreen. Easy to customize markup and CSS.
 *
 * To see a live demo, go to:
 * http://www.wimagguc.com/projects/jquery-latitude-longitude-picker-gmaps/
 *
 * by Richard Dancsi
 * http://www.wimagguc.com/
 *
 */

// for ie9 doesn't support debug console >>>
if (!window.console) window.console = {};
if (!window.console.log) window.console.log = function () { };
// ^^^

var gMapsLatLonPicker = function ()
{
    var _self = this;

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // PARAMETERS (MODIFY THIS PART) //////////////////////////////////////////////////////////////
    _self.params = {
        defLat: 0,
        defLng: 0,
        defZoom: 1,
        queryElevationWhenLatLngChanges: true,
        mapOptions: {
            mapTypeId: google.maps.MapTypeId.ROADMAP,
            mapTypeControl: false,
            disableDoubleClickZoom: true,
            zoomControlOptions: true,
            streetViewControl: false
        },
        strings: {
            markerText: "Drag this Marker",
            error_empty_field: "Couldn't find coordinates for this place",
            error_no_results: "Couldn't find coordinates for this place"
        }
    };


    ///////////////////////////////////////////////////////////////////////////////////////////////
    // VARIABLES USED BY THE FUNCTION (DON'T MODIFY THIS PART) ////////////////////////////////////
    _self.vars = {
        ID: null,
        LATLNG: null,
        map: null,
        marker: null,
        geocoder: null
    };

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // PRIVATE FUNCTIONS FOR MANIPULATING DATA ////////////////////////////////////////////////////
    var setPosition = function (position)
    {
        _self.vars.marker.setPosition(position);
        _self.vars.map.panTo(position);

        document.getElementById(_self.vars.ID + "Zoom").value = _self.vars.map.getZoom();
        document.getElementById(_self.vars.ID + "Long").value = position.lng();
        document.getElementById(_self.vars.ID + "Lat").value = position.lat();

        if (_self.params.queryElevationWhenLatLngChanges)
        {
            getElevation(position);
        }
    };

    // for getting the elevation value for a position
    var getElevation = function (position)
    {
        var latlng = new google.maps.LatLng(position.lat(), position.lng());

        var locations = [latlng];

        var positionalRequest = { 'locations': locations };

        _self.vars.elevator.getElevationForLocations(positionalRequest, function (results, status)
        {
            if (status == google.maps.ElevationStatus.OK)
            {
                if (results[0])
                {
                    document.getElementById(_self.vars.ID + "El").value = results[0].elevation;
                } else
                {
                    document.getElementById(_self.vars.ID + "El").value = "";
                }
            } else
            {
                document.getElementById(_self.vars.ID + "El").value = "";
            }
        });
    };

    // search function
    var performSearch = function (string, silent)
    {
        if (string == "")
        {
            if (!silent)
            {
                displayError(_self.params.strings.error_empty_field);
            }
            return;
        }
        _self.vars.geocoder.geocode(
            { "address": string },
            function (results, status)
            {
                if (status == google.maps.GeocoderStatus.OK)
                {
                    document.getElementById(_self.vars.ID + "Zoom").value = 15;
                    _self.vars.map.setZoom(parseFloat(document.getElementById(_self.vars.ID + "Zoom").value));
                    setPosition(results[0].geometry.location);
                } else
                {
                    if (!silent)
                    {
                        displayError(_self.params.strings.error_no_results);
                    }
                }
            }
        );
    };

    // error function
    var displayError = function (message)
    {
        alert(message);
    };

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // PUBLIC FUNCTIONS  //////////////////////////////////////////////////////////////////////////
    var publicfunc = {

        // INITIALIZE MAP ON DIV //////////////////////////////////////////////////////////////////
        init: function (prefix)
        {
            _self.vars.ID = prefix;

            _self.params.defLat = userLat ? userLat : document.getElementById(prefix + "Lat").value;
            _self.params.defLng = userLong ? userLong : document.getElementById(prefix + "Long").value;
            _self.params.defZoom = userZoom ? userZoom : parseFloat(document.getElementById(prefix + "Zoom").value);

            _self.vars.LATLNG = new google.maps.LatLng(_self.params.defLat, _self.params.defLng);

            _self.vars.MAPOPTIONS = _self.params.mapOptions;
            _self.vars.MAPOPTIONS.zoom = _self.params.defZoom;
            _self.vars.MAPOPTIONS.center = _self.vars.LATLNG;

            _self.vars.map = new google.maps.Map(document.getElementById(prefix + "Map"), _self.vars.MAPOPTIONS);
            _self.vars.geocoder = new google.maps.Geocoder();
            _self.vars.elevator = new google.maps.ElevationService();

            _self.vars.marker = new google.maps.Marker({
                position: _self.vars.LATLNG,
                map: _self.vars.map,
                title: _self.params.strings.markerText,
                draggable: true
            });

            // Set position on doubleclick
            google.maps.event.addListener(_self.vars.map, 'dblclick', function (event)
            {
                setPosition(event.latLng);
            });

            // Set position on marker move
            google.maps.event.addListener(_self.vars.marker, 'dragend', function (event)
            {
                setPosition(_self.vars.marker.position);
            });

            // Set zoom feld's value when user changes zoom on the map
            google.maps.event.addListener(_self.vars.map, 'zoom_changed', function (event)
            {
                document.getElementById(prefix + "Zoom").value = _self.vars.map.getZoom();
            });

            document.getElementById(prefix + "Preview").addEventListener('load', function ()
            {
                var center = _self.vars.map.getCenter();
                google.maps.event.trigger(_self.vars.map, 'resize');
                _self.vars.map.setCenter(center);
            });

            // Search function by search button
            document.getElementById(prefix + "Search").addEventListener("click", function ()
            {
                performSearch(document.getElementById(_self.vars.ID + "Query").value, false);
            });

            return _self.vars.map;
        }
    }

    return publicfunc;
};

function initPicker(mapPrefix)
{
    return gMapsLatLonPicker().init(mapPrefix);
}
