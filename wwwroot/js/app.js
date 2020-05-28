let HOST = "";
//let HOST = "http://rony5.atwebpages.com";
let GET_FLIGHT_PLAN_URI = HOST + "/api/FlightPlan/";
let GET_FLIGHT_URI = HOST + "/api/Flights?relative_to=";
let DELETE_FLIGHT_URI = HOST + "/api/Flights/";
let POST_FLIGHT_PLAN_URI = HOST + "/api/FlightPlan";
let BLUE_ICON = "planeIcons/plane-blue.png";
let RED_ICON = "planeIcons/plane-red.png"
let ISO_REGEX_MODIFIER = /[^.]*/m;
let ISO_REGEX_FINDER = /[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}Z/m;
let DEFAULT_INPUT_MESSAGE = "Choose JSON";
let EPSILON = 0.001;
let INTERVAL = 4000;
let ALERT_TEMPLATE = "<div class=\"alert alert-warning alert-dismissible fade show\" role=\"alert\"><strong>Error - </strong> <span id=\"error-message\"></span><button type=\"button\" class=\"close\" data-dismiss=\"alert\" aria-label=\"Close\"><span aria-hidden=\"true\">&times;</span></button></div>" 
let LI_INTERNAL_PREFIX = "<li id=\"template\" type=\"button\" class=\"list-group-item list-group-item-action\" data-toggle=\"collapse\"><button type=\"button\" class=\"close close-flight\"> <span>&times;</span></button>";
let LI_EXTERNAL_PREFIX = "<li id=\"template\" type=\"button\" class=\"list-group-item list-group-item-action\"data-toggle=\"collapse\">";
let LI_INFIX = ", &nbsp; <strong>";
let LI_POSTFIX = "</strong></li>";
let FLIGHT_ID_REGEX = /[a-zA-Z0-9]{6,10}/;
let HEADERS = {
  "Access-Control-Allow-Origin": "*",
  "Access-Control-Allow-Methods": "DELETE, POST, GET",
  "Access-Control-Allow-Headers": "Content-Type, Access-Control-Allow-Headers, Authorization, X-Requested-With",
  "Content-Type": 'application/json'
}
let minTimeHeap = null;
let internalFlightsNumber = 0;
let externalFlightsNumber = 0;
let currentFlight = null;
let map;
let mapIsSet = false;

//json of all icons on map, where key is the flight id
let markers = {};


//return true if json miss at least one required field of flightPlan object
function flightPlanMissFields(json) {
  let answer = json.passengers && json.company_name && json.initial_location && json.segments;
  
  return !answer;
}

//return true if json miss at least one required field of flight object
function flightMissFields(json) {
  let halfAnswer = json.passengers && json.company_name && json.longitude && json.latitude;
  let otherHalfAnswer = json.flight_id && json.date_time;
  let answer = json.hasOwnProperty("is_external");
  
  answer &= (!halfAnswer) && (!otherHalfAnswer);

  return answer;
}

function validateIsExternal(input) {
  return typeof input === "boolean";
}

function validateFlightId(input) {
  if (input.length <=10 && input.length >= 6) {
    return FLIGHT_ID_REGEX.test(input);
  }

  return false;
}

function validatePassengers(passengers) {
  let varType = typeof(passengers)
  let answer = false;

  if (varType === 'number') {
    answer = Number.isInteger(passengers);
  } else if (varType === 'string') {
    let n = Math.floor(Number(passengers));
    answer = n !== Infinity && String(n) === passengers && n >= 0;
  }

  return answer;
}

function validateCompanyName(name) {
  return typeof name === 'string';
}

function validateLatitude(input) {
  return ((input <= 90) && (input >= -90));
}

function validateLongitude(input) {
  return ((input <= 180) && (input >= -180));
}

function validateDateTime(input) {
  let first_match = ISO_REGEX_FINDER.exec(input);

  return (first_match != null && (first_match[0] === input));
}

function validateInitialLocation(initial_location) {
  let answer = true;

  if (!(initial_location.hasOwnProperty('latitude')
        && initial_location.hasOwnProperty('longitude')
        && initial_location.hasOwnProperty('date_time'))) {
    return false;
  }

  answer &= validateLatitude(initial_location.latitude);
  answer &= validateLongitude(initial_location.longitude);
  answer &= validateDateTime(initial_location.date_time);

  return answer;
}

function validateSegments(segments) {
  let answer = true;

  if (!Array.isArray(segments)) {
    return false;
  }

  for (item of segments) {
    answer &= validateLongitude(item.longitude);
    answer &= validateLatitude(item.latitude);
  }

  return answer;
}

function validateFlightPlan(json) {
  let allIsGood = true;

  allIsGood &= validatePassengers(json.passengers);
  allIsGood &= validateCompanyName(json.company_name);
  allIsGood &= validateInitialLocation(json.initial_location);
  allIsGood &= validateSegments(json.segments);

  return allIsGood;
}

function validateFlightPlanInput(fpString) {
  try {
    var data = JSON.parse(fpString);
    

    //check it's a json file
    if (flightPlanMissFields(data)) {
      return false;
    }

    return validateFlightPlan(data);
  }
  catch(e) {
    return false;
  }
}

function validateFlight(flight) {
  let allIsGood = true;

  if (flightMissFields(flight)) {
    return false;
  }

  allIsGood &= validateLongitude(flight.longitude);
  allIsGood &= validateLatitude(flight.latitude);
  allIsGood &= validatePassengers(flight.passengers);
  allIsGood &= validateCompanyName(flight.company_name);
  allIsGood &= validateDateTime(flight.date_time);
  allIsGood &= validateIsExternal(flight.is_external);
  allIsGood &= validateFlightId(flight.flight_id);

  return allIsGood;
}

function getCurrentTimeISO() {
  let time = new Date();

  return ISO_REGEX_MODIFIER.exec(time.toISOString()) + 'Z';
}

//convert to a more readable time string
function convertISOToTimeString(iso) {
  if (iso == null) {
    iso = getCurrentTimeISO();
  }

  let time = new Date(iso);

  let day = (time.getDate()).toString();
  let month = (time.getMonth() + 1).toString();
  let year = (time.getFullYear()).toString();
  let hours = (time.getHours()).toString();
  let minutes = (time.getMinutes()).toString();

  return day + '/' + month + '/' + year + ' ' + hours + ':' + minutes;
}

function getDistance(start, end) {
  let dx = start.latitude - end.latitude;
  let dy = start.longitude - end.longitude;
  let distance = Math.sqrt(Math.pow(dx, 2) + Math.pow(dy, 2));

  return distance;
}

function passedInSegment(point, start, end) {
  let totalLineDistance = getDistance(start, end);
  let pointToStartDistance = getDistance(point, start);

  return (pointToStartDistance / totalLineDistance);
}

function pointIsInSegment(point, lineStart, lineEnd) {
  // get line length
  let lineLength = getDistance(lineStart, lineEnd);

  //get length from coordinate to lineStart
  let pointStartLength = getDistance(point, lineStart);

  //get length from coordinate to lineEnd
  let pointEndLength = getDistance(point, lineEnd);

  return ((lineLength - (pointStartLength + pointEndLength)) < EPSILON);
}

function calculateETL(flightPlan, flight) {
  let position = {"latitude": flight.latitude, "longitude":flight.longitude};
  let start = flightPlan.initial_location
  let isFutureSegment = false;
  let etl = null;

  for (seg of flightPlan.segments) {
    if (isFutureSegment) {
      etl += seg.timespan_seconds;
    } else if (pointIsInSegment(position, start, seg)) {
      let progress = passedInSegment(position, start, seg);
      etl = progress * seg.timespan_seconds;
      isFutureSegment = true;
    }

    start = seg;
  }

  return etl;
}

function rememberEndOfFlight(flightId, duration) {
  minTimeHeap.push({"duration": duration, "id": flightId});
  setTimeout(removeFlight, duration * 1000)
}



//end of data handling
//start of map functions



//starts map with default options
function initMap() {
  map = new google.maps.Map(document.getElementById("map"), {
  center: {lat: 32.109333, lng: 34.855499},
  zoom: 3,
  gestureHandling: "greedy",
  disableDefaultUI: "true",
  minZoom: 3
});

  //to not show lines
  map.data.setStyle({"visible": false});

  map.addListener("click", function(e) {
    if (e.tb.currentTarget == null && currentFlight) {
      clearView();
    }
  });
}

function removeLineFromMap(flightId) {
  let flightLine = map.data.getFeatureById(flightId);

  if (flightLine) {
    //remove line of flight from map
    map.data.remove(flightLine);  
  }
}

function removeIconFromMap(flightId) {
  let marker = markers[flightId];

  if (marker) {
    marker.setMap(null);
    delete markers[flightId];
  }
}

function removeFlightFromMap(flightId) {
  
  removeLineFromMap(flight_id);
  removeIconFromMap(flightId);
}

/*
 * get an array of segments as in the FlighPlan Object.
 * turn each segment to a googla maps LatLng object.
 * take all of these objects and build one long line.
 */
function makeMultiLineFromSegments(segments) {
  let latLngArray = [];
  let latLngItem;
  let lat;
  let lng;

  for (coordinate of segments) {
    lat = parseFloat(coordinate.latitude);
    lng = parseFloat(coordinate.longitude);
/*    
    if (typeof coordinate.latitude === 'string') {
      lat = Number(coordinate.latitude);
    }

    if (typeof coordinate.longitude === 'string') {
      lng = Number(coordinate.longitude);
    }
*/

    latLngItem = {
      "lat": lat,
      "lng": lng
    }
    latLngArray.push(latLngItem);
  }

  return new google.maps.Data.LineString(latLngArray);
}

/*
 * get an array of segments as in the FlighPlan Object.
 * set a new element on map with id == flight id.
 */
function addLineToMap(flightId, segments) {
  if (segments) {
    let multiline = makeMultiLineFromSegments(segments);
    
    map.data.add({
      "geometry": multiline,
      "id": flightId,
      "properties": null
    });
  }
}

//get color string as argument and return blue or red icon object
function getIcon(color) {
  
  //settings for optimal size and display of plane icon
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
  let marker = markers[flightId];
  let icon = getIcon(color)

  marker.setIcon(icon);
}

function setIconLocation(flightId, lat, lng) {
  let marker = markers[flightId];
  let latLng = {"lat": lat, "lng": lng};

  marker.setPosition(latLng);
}

//make all plane icons blue and no lines are drawn on map.
function clearMap() {
  if (currentFlight != null) {
    setIconColor(currentFlight, "blue");
    markers[currentFlight].setAnimation(null);
  }

  map.data.revertStyle();
}

function displayCurrentFlightOnMapHelper(currentFlight) {
  let segmentsPromise = getSegments(currentFlight);
  segmentsPromise.then(segs => addLineToMap(currentFlight, segs));
}

//make map show line of current flight and color it's plane in red
function displayCurrentFlightOnMap() {
  let currFlightFeature = map.data.getFeatureById(currentFlight);

  if (!currFlightFeature) {
    displayCurrentFlightOnMapHelper(currentFlight);
    currFlightFeature = map.data.getFeatureById(currentFlight);
  }

  if (currFlightFeature) {
    map.data.revertStyle();
    map.data.overrideStyle(currFlightFeature, {visible: 'true'});
    setIconColor(currentFlight, "red");

    //make 2 bounces when marked
    markers[currentFlight].setAnimation(google.maps.Animation.BOUNCE);
    setTimeout(function(){ markers[currentFlight].setAnimation(null); }, 750);
  }
}

//adds a blue plane icon to map and add click event handler
function addPlaneIcon(flightId, latitude, longitude) {
  let icon = getIcon("blue");
  //let point = new google.maps.Data.Point();

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



//end of map
//start of server functions

function getContent(promise) {
  if (promise.constructor === Array) {
    return promise[0];
  }
  else if (typeof promise === "string") {
    return JSON.parse(promise);
  } else {
    return promise;
  }
}

async function doAjaxGet(uri) {
  // uri = uri + "&unlimited";
  let resJson, data, res;
  
  try {
    res = await fetch(uri, {
        "headers": HEADERS,
        "method": 'GET'
      });
    } catch (err) {
      raiseErrorToClient("Problem in fetch GET command.");
      return;
    }

  try {
    resJson = await res.json();
    data = await getContent(resJson);
  } catch (arr) {
    raiseErrorToClient("Problem in message from server.");
  }

  return data;   
}

async function doAjaxPost(message) {
  // uri = uri + "&unlimited";
  try {
    let msg = await fetch(POST_FLIGHT_PLAN_URI, {
        "method": 'POST',
        "headers": HEADERS,
        "body": message
      });

      return msg;
    }
   catch (err) {
    raiseErrorToClient("Problem in fetch POST command.");
  }
}

function getFlightObjectPromise(flightId) {
  let uri = GET_FLIGHT_URI+flightId;

  return doAjaxGet(uri);
}

function getAllFlightsPromise() {
  let currentTime = getCurrentTimeISO();
  let flightJson = null;
  let uri = GET_FLIGHT_URI + currentTime + "&sync_all";

  flightJson = doAjaxGet(uri)

  return flightJson;
}

function getFlightPlanPromise(flightId) {
  let flightPlan = null;
  let uri = GET_FLIGHT_PLAN_URI + flightId;    
    
  flightPlan = doAjaxGet(uri);

  return flightPlan;
}

function getSegmentsPromise(flightId) {
    let flightPlanPromise = getFlightPlanPromise(flightId);
    let segments = null;


    flightPlanPromise.then(json => {return json.segments;});
}

function uploadFlightPlan(flightPlanString) {
 if (validateFlightPlanInput(flightPlanString)) {
    return answer = doAjaxPost(flightPlanString);
  } else {
   raiseErrorToClient("Invalid Json file was uploaded.")
  }
}

async function removeFlightFromServer(flightId) {
  try {
    let msg = await fetch(DELETE_FLIGHT_URI+flightId + "&unlimited", {
        "method": 'DELETE',
        "headers": HEADERS
      });

      return msg;
    }


   catch (err) {
    raiseErrorToClient("Network Error during fetch.");
  }
}

function updateFlightsFromArray(flights) {
  if (flights) {
    if (flights.constructor !== Array) {
      flights = [flights];
    }

    for(flight of flights) {
      addFlight(flight);
    }
  }
}

function updateFlightsFromServer() {
  let serverFlightsPromise = getAllFlightsPromise();

  try {
    serverFlightsPromise.then(flights => updateFlightsFromArray(flights));
  } catch(err) {
    raiseErrorToClient("Could not get flights from server.");
  }
}



//end of server
//start of code for bootstrap and view functions




// disable active list item and clear map from recent active flight
function resetCurrentFlight() {
  $('#'+currentFlight).removeClass('active');
  clearMap();
}

//make all page to show no specific flight
function clearView() {
  $("#show-flight").hide();
  $("#show-no-flight").show();

  if (currentFlight != null && mapIsSet) {
    resetCurrentFlight()
  }

  currentFlight = null;
}

//remove list item and adjust lists counters
function removeFlightFromList(flightId) {
  let list = $('#'+flightId).closest('ul');

  if ($(list).attr('id') == "internal-flights-list") {
    internalFlightsNumber--;
    $('#in-flights-badge').text(internalFlightsNumber);
  } else {
    externalFlightsNumber--;
    $('#ex-flights-badge').text(externalFlightsNumber);
  }

  $('#'.concat(String(flightId))).remove();
}

//set all fields of the flight details card
function setFlightInfoCard(flightJson) {
  $("#flight-id").text(flightJson.flight_id);
  $("#flight-company").text(flightJson.company_name);
  $("#flight-passengers-number").text(flightJson.passengers);
  $("#flight-time").text(flightJson.date_time);
  $("#flight-latitude").text(flightJson.initial_location.latitude);
  $("#flight-longitude").text(flightJson.initial_location.longitude);
}

function getIDFromList() {
  let min = minTimeHeap.pop();
  let id = min.id;

  //flightId in map
  if (markers[id]) {
    return id;
  }

  return getIDFromList();
}

//remove flight from page and post delete to server
function removeFlight(flightId = null) {
  let fromTimeOut = !flightId;

  //set flightId from minimum heap
  if (fromTimeOut) {
    flightId = getIDFromList();
  }

  if (flightId == currentFlight) {
    clearView();
  }

  removeFlightFromList(flightId);
  removeFlightFromMap(flightId);

  //user deleted explicitely
  if (!fromTimeOut) {
    removeFlightFromServer(flightId);
  }
}

//event handler - user clicked on close icon
function clickcloseFlight(e) {
  let flightId = $((e.target).closest('li')).attr('id');
  let flightIsDisplayed = (flightId == currentFlight);
  let ct = e.currentTarget;

    //delete only if it's not current displayed flight
    if (!flightIsDisplayed) {
      removeFlight(flightId);
    } else {
      clearView();
    }
  e.stopPropagation();
}

function updateFlightCard(flight) {
  setFlightInfoCard(flight);
  $("#show-no-flight").hide();
  $("#show-flight").show();
}

//display flight on GUI
function showFlight(flightId) {
  if (currentFlight != null) {
    resetCurrentFlight();
  }

  $('#'+ flightId).addClass("active");
  currentFlight = flightId;
  displayCurrentFlightOnMap();
  
  try {
    let flightPromise = getFlightObjectPromise(flightId);

    flightPromise.then(flight => displayFlightCard(flight));
  } catch (err) {
    raiseErrorToClient("Problem in getting a flight object.")
  }
}

function handleActiveFlight(flightId, etl, lat, lng) {
  if (etl <= 0) {
    removeFlight(flightId);
  } else {
    setIconLocation(flightId, lat, lng);
  }
}

function updateFlight(flight) {
  let lat = parseFloat(flight.latitude);
  let lng = parseFloat(flight.longitude);
  let planPromise = doAjaxGet(GET_FLIGHT_PLAN_URI + flight.flight_id);
  let etlPromise = planPromise.then(plan => calculateETL(plan, flight));

  etlPromise.then(etl => handleActiveFlight(flight.flight_id, etl, lat, lng));
}

/*
 * helper of addFlightToList function.
 * add a list item event handlers of closing and clicking
 */
function bindLiEventHandlers(flightId) {
  let childCloseButton;

  $("#template").attr("id", flightId);
  $("#" + flightId).on("click", function (e) {
    let flightId = $(e.currentTarget).attr('id');
    showFlight(flightId);
  });

  childCloseButton = $("#" + flightId).children('.close-flight')

  childCloseButton.on("click", clickcloseFlight);
}

//add new flight to list in GUI
function addFlightToList(flight) {

  //same ending for both list item HTML
  let content = flight.flight_id + LI_INFIX + flight.company_name + LI_POSTFIX;
  
  if (flight.is_external) {
    content = LI_EXTERNAL_PREFIX + content;
    $("#external-flights-list").append(content);
    externalFlightsNumber++;
    $('#ex-flights-badge').text(externalFlightsNumber);
  } else {
    content = LI_INTERNAL_PREFIX + content;
    $("#internal-flights-list").append(content);
    internalFlightsNumber++;
    $('#in-flights-badge').text(internalFlightsNumber);
  }

  bindLiEventHandlers(flight.flight_id);
}

function addIconAtCurrentLocation(flightId) {
  let flightPromise = getFlightObjectPromise(flightId);

  try {
    flightPromise.then(flight => addPlaneIcon(flightId, flight.latitude, flight.longitude));
  } catch (err) {
    raiseErrorToClient("Problem in getting flight" + flightId + "from Server.");
  }
}

function addFlightHelper(flightPlan, flight) {
  if (validateFlightPlan(flightPlan)) {
    let etl = calculateETL(flightPlan, flight)

    addFlightToList(flight);
    addLineToMap(flight.flight_id, flightPlan.segments);
    rememberEndOfFlight(flight.flight_id, etl);
    addIconAtCurrentLocation(flight.flight_id);
  }
}

function addFlight(flightObject) {
  if (!validateFlight(flightObject)) {
    raiseErrorToClient("Got invalid flight object.");
    return;
  }

  let flightId = flightObject.flight_id;

  if (!markers.hasOwnProperty(flightId)) {
    let flightPlanPromise = getFlightPlanPromise(flightId);
  
    try {
      flightPlanPromise.then(flightPlan => addFlightHelper(flightPlan, flightObject));
    } catch (err) {
      raiseErrorToClient("Unable to add new flight.")
    }    
  } else {
    updateFlight(flight);
  }
}

//create new alert, change content and show it
function raiseErrorToClient(message) {
  
  //to create alert object if not exist
  if (!$("#error-message").length) {
    $("#error-displayer").html(ALERT_TEMPLATE);
  }

  $("#error-message").text(message);
  $("#error-displayer").show();
}

function initData() {
  $('#in-flights-badge').text(internalFlightsNumber);
  $('#ex-flights-badge').text(externalFlightsNumber);

  minTimeHeap = new Heap(function(a,b) {
    return a.duration - b.duration;
  });

  updateFlightsFromServer();
}

function initHideElements() {
  $("#error-displayer").hide();
  $("#show-flight").hide();
}

//called when click on submit button
function fileFromUser(e) {
  let myJson;
  let file = $("#inputGroupFile01").prop("files")[0];

  $("#input-label").text(DEFAULT_INPUT_MESSAGE);

  let reader = new FileReader();

  reader.onload = function(event) {
    let ans = uploadFlightPlan(event.target.result);

    //to update flights after request was made
    ans.then(() => updateFlightsFromServer());
   };

   reader.readAsText(file);
}

//bind event handlers and set
function initEventHandlers() {
  $("#inputGroupFileAddon01").bind('click', fileFromUser);
  setInterval(updateFlightsFromServer, INTERVAL);
}

//handler for changing file in file element
$(function () {
 $("#inputGroupFile01").change(function(e) {
  let item = $("#inputGroupFile01").prop("files");
  let file_name = ($("#inputGroupFile01").prop("files")[0]).name
  $("#input-label").text(file_name);
 })
});

//handler for clicking on internal flights
$(function() {
  $("#in-flights-heading").click( function(e) {
      if (internalFlightsNumber) {
        $("#in-flights-badge").fadeToggle()
      }
  })
});

//handler for clicking on internal flights
$(function() {
  $("#ex-flights-heading").click( function(e) {
    if (externalFlightsNumber) {
      $("#ex-flights-badge").fadeToggle();  
    }
  })
});

//is called from google maps API
function runMap() {
    mapIsSet = true;
    initMap();
}

//main function - initialization and configuration
$(function () {
  initHideElements();
  initData();
  initEventHandlers();
  updateFlightsFromServer();
});