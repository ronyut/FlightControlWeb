/*
 * script for data in project - containt global variables and functions
 * for handling data.
 * written by Yehonatan Sofri in May 2020.
 */

let GET_FLIGHT_PLAN_URI = "api/FlightPlan/";
let GET_FLIGHT_URI = "api/Flights?relative_to=";
let DELETE_FLIGHT_URI = "api/Flights/";
let POST_FLIGHT_PLAN_URI = "api/FlightPlan";
let BLUE_ICON = "planeIcons/plane-blue.png";
let RED_ICON = "planeIcons/plane-red.png"
let ISO_REGEX_MODIFIER = /[^.]*/m;
let ISO_REGEX_FINDER = /[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}Z/m;
let DEFAULT_INPUT_MESSAGE = "Drop json";
let EPSILON = 0.001;
let INTERVAL = 1000;
let ANIMATION_DURATION = 750;
let MIN_ID_LENGTH = 6;
let MAX_ID_LENGTH = 10;
let ALERT_TEMPLATE = "<div class=\"alert alert-warning alert-dismissible fade"
                     + " show\" role=\"alert\"><strong>Error - </strong> <span"
                     + " id=\"error-message\"></span><button type=\"button\""
                     + " class=\"close\" data-dismiss=\"alert\" aria-label=\""
                     + "Close\"><span aria-hidden=\"true\">&times;</span></"
                     + "button></div>"
let LI_INTERNAL_PREFIX = "<li id=\"template\" type=\"button\" class=\""
                         + "list-group-item list-group-item-action\" "
                         + "data-toggle=\"collapse\"><button type=\"button"
                         + "\" class=\"close close-flight\"> <span>&times;"
                         + "</span></button>";
let LI_EXTERNAL_PREFIX = "<li id=\"template\" type=\"button\" class=\""
                         + "list-group-item list-group-item-action\"data-toggle"
                         + "=\"collapse\">";
let LI_INFIX = ", &nbsp; <strong>";
let LI_POSTFIX = "</strong></li>";
let FLIGHT_ID_REGEX = /[a-zA-Z0-9]{6,10}/;
let HEADERS = {
  "Content-Type": 'application/json'
};
let minTimeHeap = null;
let internalFlightsNumber = 0;
let externalFlightsNumber = 0;
let currentFlight = null;
let map;
let mapIsSet = false;

// json of all icons on map, where key is the flight id.
let markers = {};


// return true if json miss at least one required field of flightPlan object.
function flightPlanMissFields(json) {
  let halfAnswer = json.passengers && json.company_name;
  let otherHalf = json.initial_location && json.segments;

  return !(halfAnswer && otherHalf);
}

// return true if json miss at least one required field of flight object.
function flightMissFields(json) {
  let halfAnswer = json.passengers && json.longitude && json.latitude;
  let otherHalfAnswer = json.flight_id && json.date_time && json.company_name;
  let answer = json.hasOwnProperty("is_external");
  
  answer &= Boolean(halfAnswer && otherHalfAnswer);

  return !answer;
}

function validateIsExternal(input) {
  return typeof input === "boolean";
}

function validateFlightId(input) {
  if (input.length <= MAX_ID_LENGTH && input.length >= MIN_ID_LENGTH) {
    return (typeof input === 'string');
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

// make sure flight plan json is valid.
function validateFlightPlan(json) {
  if (!json) return false;

  let allIsGood = true;

  allIsGood &= validatePassengers(json.passengers);
  allIsGood &= validateCompanyName(json.company_name);
  allIsGood &= validateInitialLocation(json.initial_location);
  allIsGood &= validateSegments(json.segments);

  return allIsGood;
}

// validate json of flightplan uploaded by user.
function validateFlightPlanInput(fpString) {
  try {
    var data = JSON.parse(fpString);
    
    // check it's a json file
    if (flightPlanMissFields(data)) {
      return false;
    }

    return validateFlightPlan(data);
  }
  catch(e) {
    return false;
  }
}

// make sure flight object has valid data.
function validateFlight(flight) {
  let allIsGood = true;

  if (flightMissFields(flight)) return false;

  allIsGood &= validateLongitude(flight.longitude);
  allIsGood &= validateLatitude(flight.latitude);
  allIsGood &= validatePassengers(flight.passengers);
  allIsGood &= validateCompanyName(flight.company_name);
  allIsGood &= validateDateTime(flight.date_time);
  allIsGood &= validateIsExternal(flight.is_external);
  allIsGood &= validateFlightId(flight.flight_id);

  return allIsGood;
}

// return time in UTC without milliseconds.
function getCurrentTimeISO() {
  let time = new Date();

  return ISO_REGEX_MODIFIER.exec(time.toISOString()) + 'Z';
}

// convert to a more readable time string.
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

   if (minutes.length == 1) {
      minutes = "0" + minutes;
   }

  return day + '/' + month + '/' + year + ' ' + hours + ':' + minutes;
}

// return distance between 2 input point of latitude & longitude.
function getDistance(start, end) {
  let dx = start.latitude - end.latitude;
  let dy = start.longitude - end.longitude;
  let distance = Math.sqrt(Math.pow(dx, 2) + Math.pow(dy, 2));

  return distance;
}

// return the proportion of the passed length in segment.
function progressInSegment(point, start, end) {
  let totalLineDistance = getDistance(start, end);
  let pointToStartDistance = getDistance(point, start);

  return (pointToStartDistance / totalLineDistance);
}

// return true if input point is on line of 2 points.
function pointIsInSegment(point, lineStart, lineEnd) {
  // get line length
  let lineLength = getDistance(lineStart, lineEnd);

  // get length from coordinate to lineStart.
  let pointStartLength = getDistance(point, lineStart);

  // get length from coordinate to lineEnd.
  let pointEndLength = getDistance(point, lineEnd);

  return ((lineLength - (pointStartLength + pointEndLength)) < EPSILON);
}

// return estimated time left by calculating position & future segments.
function calculateETL(flightPlan, flight) {
  let position = {"latitude": flight.latitude, "longitude":flight.longitude};
  let start = flightPlan.initial_location;
  let isFutureSegment = false;
  let etl = null;

  for (seg of flightPlan.segments) {
    if (isFutureSegment) {
      etl += seg.timespan_seconds;
    } else if (pointIsInSegment(position, start, seg)) {
      let progress = progressInSegment(position, start, seg);
      etl = progress * seg.timespan_seconds;
      isFutureSegment = true;
    }

    start = seg;
  }

  return etl;
}

// set a timeout event when flight ended to remove flight from GUI.
function syncEndOfFlight(flightId, duration) {
  setTimeout(function() {
    removeFlight(flightId, false);
  }, duration * 1000);
}

function getFlightFromArray(flights, flightId) {
  if (!(flights && flightId)) return false;

  if (flights.constructor !== Array) {
    flights = [flights];
  }

  for (flight of flights) {
    let tmp_id = flight.flight_id;

    if (tmp_id === flightId) return flight;
  }
  return false;
}
