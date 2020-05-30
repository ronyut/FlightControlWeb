/*
 * script for Air Control App for AP2 course. Synchronize html and events
 * in GUI.
 * written by Yehonatan Sofri in May 2020.
 */

// disable active list item and clear map from recent active flight.
function resetCurrentFlight() {
  $('#'+currentFlight).removeClass('active');
  clearMap();
}

// make all page to show no specific flight
function clearView() {
  $("#show-flight").hide();
  $("#show-no-flight").show();

  if (currentFlight != null && mapIsSet) {
    resetCurrentFlight()
  }

  currentFlight = null;
}

// remove list item and adjust lists counters.
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

function setCardLocation(lat, lng) {
  $("#flight-latitude").text(lat);
  $("#flight-longitude").text(lng);
}

function setCardTime(iso_time) {
  let time_string = convertISOToTimeString(iso_time);

  $("#flight-time").text(time_string);
}

// set all fields of the flight details card.
function setFlightInfoCard(flightJson) {
  $("#flight-id").text(flightJson.flight_id);
  $("#flight-company").text(flightJson.company_name);
  $("#flight-passengers-number").text(flightJson.passengers);
  setCardTime(flightJson.date_time);
  setCardLocation(flightJson.latitude, flightJson.longitude);
}

/*
 * get flight id and boolean. if boolean is true a delete request
 * sent to server.
 * in all cases, remove flight from list and from map.
 */
function removeFlight(flightId, removeFromServer) {

  if (flightId == currentFlight) {
    clearView();
  }
  if (markers[flightId]) {
    removeFlightFromList(flightId);
    removeFlightFromMap(flightId);
    if (removeFromServer) {
      removeFlightFromServer(flightId);  
    }
  }
}

// event handler for when user clicks on close flight icon.
function clickcloseFlight(e) {
  let flightId = $((e.target).closest('li')).attr('id');
  let flightIsDisplayed = (flightId == currentFlight);
  let ct = e.currentTarget;

    //delete only if it's not current displayed flight
    if (!flightIsDisplayed) {
      removeFlight(flightId, true);
    } else {
      clearView();
    }
  e.stopPropagation();
}

function setAndDisplayFlightCard(flight) {
  setFlightInfoCard(flight);
  $("#show-no-flight").hide();
  $("#show-flight").show();
}

// get a flight object and display flight on map and flight details card.
function showFlightHelper(flight) {
  if (flight) {
    let latLng = getLatLng(flight.latitude, flight.longitude);

    setAndDisplayFlightCard(flight);
    displayCurrentFlightOnMap(latLng);
  }
}

// display flight on GUI.
function showFlight(flightId) {
  if (currentFlight != null) {
    resetCurrentFlight();
  }

  $('#'+ flightId).addClass("active");
  currentFlight = flightId;
    
  try {
    let flightPromise = getFlightObjectPromise(flightId);
    flightPromise.then(flight => showFlightHelper(flight));
  } catch (err) {
    raiseErrorToClient("Problem in getting a flight object.")
  }
}

// if not ended - update icon location, else remove from GUI.
function handleActiveFlight(flightId, etl, lat, lng) {
  if (etl <= 0) {
    removeFlight(flightId, false);
  } else {
    setIconLocation(flightId, lat, lng);

    if (flightId === currentFlight) {
      setCardLocation(lat, lng);
    }
  }
}

// get flight object of active flight and update GUI.
function updateFlight(flight) {
  let lat = parseFloat(flight.latitude);
  let lng = parseFloat(flight.longitude);
  let planPromise = getFlightPlanPromise(flight.flight_id);
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

// add new flight to list element.
function addFlightToList(flight) {
  if (!$("#"+flight.flight_id).length) {
    // same ending for both list item HTML.
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
}

function addIconAtCurrentLocationHelper(flight) {
  let lat = parseFloat(flight.latitude);
  let lng = parseFloat(flight.longitude);

  addPlaneIcon(flight.flight_id, lat, lng);
}

function addIconAtCurrentLocation(flightId) {
  let flightPromise = getFlightObjectPromise(flightId);

  try {
    flightPromise.then(flight => addIconAtCurrentLocationHelper(flight));
  } catch (err) {
    raiseErrorToClient("Problem in getting flight" + flightId
                        + "from Server.");
  }
}

// get a fligh plan and add it to GUI.
function addFlightHelper(flightPlan, flight) {
  if (validateFlightPlan(flightPlan)) {
    let etl = calculateETL(flightPlan, flight)

    addFlightToList(flight);
    addLineToMap(flight.flight_id, flightPlan.initial_location,
                 flightPlan.segments);
    syncEndOfFlight(flight.flight_id, etl);
    addIconAtCurrentLocation(flight.flight_id);
  }
}

function addFlight(flight) {
  if (!validateFlight(flight) && !flight.msg) {
    raiseErrorToClient("Got invalid flight object.");
    return;
  }

  if (!markers.hasOwnProperty(flight.flight_id)) {
    let flightPlanPromise = getFlightPlanPromise(flight.flight_id);
  
    try {
      flightPlanPromise.then(flightPlan => addFlightHelper(flightPlan,
                                                           flight));
    } catch (err) {
      raiseErrorToClient("Unable to add new flight.")
    }    
  } else {
    updateFlight(flight);
  }
}

// create new alert, change content and show it.
function raiseErrorToClient(message) {
  
  // to create alert object if not exist.
  if (!$("#error-message").length) {
    $("#error-displayer").html(ALERT_TEMPLATE);
  }

  $("#error-message").text(message);
  $("#error-displayer").show();
}

function initData() {
  $('#in-flights-badge').text(internalFlightsNumber);
  $('#ex-flights-badge').text(externalFlightsNumber);
  $("#input-label").text(DEFAULT_INPUT_MESSAGE);
}

function initHideElements() {
  $("#error-displayer").hide();
  $("#show-flight").hide();
}

// handler for changing content in file element.
function changeContent(e) {
  let file_name = ($("#inputGroupFile01").prop("files")[0]).name;

  $("#input-label").text(file_name);
}

// called when click on submit button.
function fileFromUser(e) {
  let file = $("#inputGroupFile01").prop("files")[0];

  $("#input-label").text(DEFAULT_INPUT_MESSAGE);

  let reader = new FileReader();

  reader.onload = function(event) {
    uploadFlightPlan(event.target.result);
  };

  try {
    reader.readAsText(file);
  } catch (err) {
    raiseErrorToClient("No file was added.");
  }
}

//bind event handlers and set interval of update from server
function initHandlersAndInterval() {
  $("#inputGroupFileAddon01").bind('click', fileFromUser);
  $("#inputGroupFile01").bind('change', changeContent);
  setInterval(updateFlightsFromServer, INTERVAL);
}

//handler for clicking on internal flights
$(function() {
  $("#in-flights-heading").click( function(e) {
      if (internalFlightsNumber) {
        $("#in-flights-badge").fadeToggle()
      }
  });
});

//handler for clicking on internal flights
$(function() {
  $("#ex-flights-heading").click( function(e) {
    if (externalFlightsNumber) {
      $("#ex-flights-badge").fadeToggle();  
    }
  })
});

//main function - initialization and configuration
$(function () {
  initHideElements();
  initData();
  initHandlersAndInterval();
});