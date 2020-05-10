// my javascript file
let path="";
let internal_flights_number = 3;
let external_flights_number = 3;
let current_flight = null;

document.getElementById('internal_flights_n').innerHTML = internal_flights_number;
document.getElementById('external_flights_n').innerHTML = external_flights_number;

function removeFlight(flight_id) {
  $('#'.concat(String(flight_id))).remove();
  //external/ internal_flights_number--;

  //remove from map
  //reset flight view
  //update server
}

function getFlightFromServer(flight_id) {

}

function getFlightPlanFromServer(flight_id) {

}

function emptyScreen() {
  //hide Delete button
}

function showFlight() {

}

function uploadFlight() {

}

function addFlight(flight) {

}

//when removing a flight item from flight list
$(function() {
  $(".close").click(function(){
    let flight_id = $(this).closest('li').attr('id')
    removeFlight(flight_id);
  });
});

$(function() {

});
