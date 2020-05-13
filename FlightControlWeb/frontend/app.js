let path="";
let internal_flights_number = 0;
let external_flights_number = 0;
let current_flight = null;


function clearView() {
  $("#show_flight").hide();
  $("#show_no_flight").show();

  if (current_flight != null) {
    $('#'.concat(String(current_flight))).removeClass('active');
  }

  current_flight = null;

  //something with map
}

//remove list item and adjust lists counters
function removeFlightFromList(flight_id) {
  if ($('#'.concat(String(current_flight))).attr('id') == "in_flights_heading") {
    internal_flights_number--;
  } else {
    external_flights_number--;
  }

  $('#'.concat(String(flight_id))).remove();
}

function removeFlightFromMap(flight_id) {
  //
}

function removeFlight(flight_id) {
  removeFlightFromList(flight_id);
  removeFlightFromMap(flight_id);
  //postFlightDeleted(flight_id);
}

//happans if user clicked X
function clickcloseFlight(e) {
  let flight_id = $((e.target).closest('li')).attr('id');  
  let flightIsDisplayed = (flight_id == current_flight);

  //flight is not shown right screen
  if (!flightIsDisplayed) {
    removeFlight(flight_id);
  }

  clearFlightView();
  e.stopPropagation();
}

function getFlightFromServer(flight_id) {

}

function getFlightPlanFromServer(flight_id) {

}

function showFlight(flight_id) {
  $(e.target).addClass("active");

  current_flight = flight_id;

  //get data from server

  //show flight screen
  //show loading screen??
  //check if shown
  //get parameters from server
  //set parameters
  //show flight screen
  //update current_flight
}

function listClickOnFlight(e) {
  let flight_id = $(e.target).attr('id');
  showFlight(flight_id);
}

function uploadFlight() {

}

function addFlight(flight) {

}

function connectToServer() {

}

function initData() {
  $('#in_flights_badge').text(internal_flights_number);
  $('#ex_flights_badge').text(external_flights_number);
}

function initHideElements() {
  $("#error_displayer").hide();
  $("#show_flight").hide();
}

function raiseErrorToClient(message) {
  $("#error_message").text(message);
  $("#error_message").fadeIn();
}

// from now on - binding event handlers and main in bottom

$(function() {
  $("#in_flights_heading").click( function(e) {
      $("#in_flights_badge").fadeToggle()
  })
});

$(function() {
  $("#ex_flights_heading").click( function(e) {
    $("#ex_flights_badge").fadeToggle();
  })
});

$(function () {
  $("error_message_button").click( function (e) {
    $("#error_message").fadeOut();
  })
});

$(function () {
  $("li").click(function(e) {
    let flight_id = $(e.target).attr('id');
    showFlight(flight_id);
  });
})

function initSetEventHandlers() {
  $(".close-flight").on("click", clickcloseFlight);
  $("li").on("click", listClickOnFlight);
}

$(function () {
  //set data
  //hide elemnts
  //connect to server
  initData();
  initHideElements();
  initSetEventHandlers();
  connectToServer();
});