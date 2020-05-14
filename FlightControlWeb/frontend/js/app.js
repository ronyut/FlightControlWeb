let path="";
let internalFlightsNumber = 3;
let externalFlightsNumber = 3;
let currentFlight = null;



function clearView() {
  $("#show-flight").hide();
  $("#show-no-flight").show();

  if (currentFlight != null) {
    $('#' + currentFlight).removeClass('active');
  }

  currentFlight = null;

  //something with map
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

function removeFlightFromMap(flightId) {
  //
}

function removeFlight(flightId) {
  removeFlightFromList(flightId);
  removeFlightFromMap(flightId);
  //postFlightDeleted(flightId);
}

//happans if user clicked X
function clickcloseFlight(e, flightId) {
  let flightIsDisplayed = (flightId == currentFlight);

  //flight is not shown right screen
  if (!flightIsDisplayed) {
    removeFlight(flightId);
  }

  clearView();
  e.stopPropagation();
}

function getFlightFromServer(flightId) {
  //update counters
}

function getFlightPlanFromServer(flightId) {

}

function showFlight(flightId) {
  if (currentFlight != flightId) {
    if (currentFlight != null) {
      $('#'+currentFlight).removeClass('active');
    }

    $('#'+ flightId).addClass("active");

    currentFlight = flightId;

    //get data from server
    //show loading screen??
    //get parameters from server
    //set parameters
    //show flight screen
  }
}

function validateFlightPlanInput() {

}

function uploadFlight(e) {

  validateFlightPlanInput();

  $.ajax({

  url: '/api/flightplan',
  type: 'POST',
  data: new FormData($('form')[0]),
  cache: false,
  contentType: false,
  processData: false,

  xhr: function () {
    let myXhr = $.ajaxSettings.xhr();
    
    return myXhr;
  }
});
}

function addFlight(flight) {

}

function connectToServer() {

}

function initData() {
  $('#in-flights-badge').text(internalFlightsNumber);
  $('#ex-flights-badge').text(externalFlightsNumber);
}

function initHideElements() {
  $("#error-displayer").hide();
  $("#show-flight").hide();
}

function raiseErrorToClient(message) {
  $("#error-message").text(message);
  $("#error-message").fadeIn();
}

// from now on - binding event handlers and main in bottom

$(function() {
  $("#in-flights-heading").click( function(e) {
      $("#in-flights-badge").fadeToggle()
  })
});

$(function() {
  $("#ex-flights-heading").click( function(e) {
    $("#ex-flights-badge").fadeToggle();
  })
});

$(function () {
  $("error-message-button").click( function (e) {
    $("#error-message").fadeOut();
  })
});

$(function () {
  $(".close-flight").click(function(e) {
    let flightId = $((e.target).closest('li')).attr('id');
    clickcloseFlight(e, flightId);
  })
});

$(function () {
  $("li").click(function(e) {
    let flightId = $(e.target).attr('id');
    showFlight(flightId);
  });
})

function initSetEventHandlers() {
  $(".close-flight").on("click", clickcloseFlight);
  $("#input-group-file-addon").on('click', uploadFlight)
}

//main function - initialization and configuration
$(function () {
  initData();
  initHideElements();
  initSetEventHandlers();
  connectToServer();
});