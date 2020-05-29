/*
 * script for handling map operations - adding icons, change center etc.
 * written by Yehonatan Sofri in May 2020.
 */

// starts map with default options.
function initMap() {
  map = new google.maps.Map(document.getElementById("map"), {
  center: {lat: 32.109333, lng: 34.855499},
  zoom: 3,
  gestureHandling: "greedy",
  disableDefaultUI: "true",
  minZoom: 3
});

  // to not show lines.
  map.data.setStyle({"visible": false});

  map.addListener("click", function(e) {
    if (e.tb.currentTarget == null && currentFlight) {
      clearView();
    }
  });
}

function removeLineFromMap(flightId) {
  if (mapIsSet) {
    let flightLine = map.data.getFeatureById(flightId);

    if (flightLine) {
      map.data.remove(flightLine);  
    }
  }
}

function removeIconFromMap(flightId) {
  let marker = markers[flightId];

  if (marker && mapIsSet) {
    marker.setMap(null);
    delete markers[flightId];
  }
}

function removeFlightFromMap(flightId) {
  if (mapIsSet) {
    removeLineFromMap(flightId);
    removeIconFromMap(flightId);
  }
}

// is called from google maps API.
function runMap() {
  mapIsSet = true;
  initMap();
}

function getLatLng(latitude, longitude) {
  let lat = parseFloat(latitude);
  let lng = parseFloat(longitude);

  return {
    "lat": lat,
    "lng": lng
  }
}

/*
 * get an array of segments as in the FlighPlan Object.
 * turn each segment to a google maps LatLng object.
 * take all of these objects and build one long line.
 */
function makeMultiLineFromSegments(initial_location, segments) {
  let latLngArray = [];
  let latLngItem;
  let lat, lng;

  if (!mapIsSet) return

  latLngArray.push(getLatLng(initial_location.latitude,
                             initial_location.longitude));

  for (coordinate of segments) {
    latLngItem = getLatLng(coordinate.latitude, coordinate.longitude);

    latLngArray.push(latLngItem);
  }

  return new google.maps.Data.LineString(latLngArray);
}

/*
 * get an array of segments as in the FlighPlan Object.
 * set a new element on map with id == flight id.
 */
function addLineToMap(flightId, initial_location, segments) {
  if (segments && mapIsSet) {
    let multiline = makeMultiLineFromSegments(initial_location, segments);
    
    map.data.add({
      "geometry": multiline,
      "id": flightId,
      "properties": null
    });
  }
}

// get color string as argument and return blue or red icon object.
function getIcon(color) {
  if (!mapIsSet) return

  // settings for optimal size and display of plane icon
  let icon = {
    "scaledSize": new google.maps.Size(20, 20),
    "origin": new google.maps.Point(0,0),
    "anchor": new google.maps.Point(20, 20)
  };

  if (color == "blue") {
    icon.url = BLUE_ICON;
  } else {
    icon.url = RED_ICON;
  }

  return icon;
}

function setIconColor(flightId, color) {
  if (mapIsSet) {
    let marker = markers[flightId];
    let icon = getIcon(color)

    marker.setIcon(icon);
  }
}

function setIconLocation(flightId, lat, lng) {
  if (mapIsSet) {
    let marker = markers[flightId];
    let latLng = {"lat": lat, "lng": lng};

    marker.setPosition(latLng);
  }
}

// make all plane icons blue and no lines displayed on map.
function clearMap() {
  if (!mapIsSet) return;

  if (currentFlight != null) {
    setIconColor(currentFlight, "blue");
    markers[currentFlight].setAnimation(null);
  }
  
  map.data.revertStyle();
}

// get flight id and draw flight plan on map.
function lineMakingHelper(currentFlight) {
  let segmentsPromise = getSegments(currentFlight);

  segmentsPromise.then(segs => addLineToMap(currentFlight, segs));
}

// make map show line of current flight and color it's plane in red.
function displayCurrentFlightOnMap(latLng) {
  if (!mapIsSet) return

  let currFlightFeature = map.data.getFeatureById(currentFlight);

  if (!currFlightFeature) {
    lineMakingHelper(currentFlight);
    currFlightFeature = map.data.getFeatureById(currentFlight);
  }

  if (currFlightFeature) {
    map.data.revertStyle();
    map.data.overrideStyle(currFlightFeature, {visible: 'true'});
    setIconColor(currentFlight, "red");
    map.panTo(latLng);

    // bounce animation of icon.
    markers[currentFlight].setAnimation(google.maps.Animation.BOUNCE);
    setTimeout(function() {
                 markers[currentFlight].setAnimation(null);
               }, ANIMATION_DURATION);
  }
}

// adds a blue plane icon to map and add click event handler.
function addPlaneIcon(flightId, latitude, longitude) {
  if (!markers[flightId] && mapIsSet) {
    let icon = getIcon("blue");
    let marker = new google.maps.Marker({
      "position": {"lat": latitude, "lng": longitude},
      "map": map,
      "icon": icon
    });

    marker.addListener("click", function() {
      showFlight(flightId);
    });

    markers[flightId] = marker;
  }
}