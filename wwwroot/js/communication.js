/*
 * script for communcation operations - functions and helpers for
 * sending ajax requests.
 * written by Yehonatan Sofri in May 2020.
 */

// convert promise of data and convert it to a json.
function getContent(promise) {
  if (typeof promise === "string") {
    return JSON.parse(promise);
  } else {
    return promise;
  }
}

// a generic get from server. 
async function doAjaxGet(uri) {
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

// function for postin flight plan.
async function doAjaxPost(message) {
  uri = POST_FLIGHT_PLAN_URI;
  try {
    let msg = await fetch(POST_FLIGHT_PLAN_URI, {
        "method": 'POST',
        "headers": HEADERS,
        "body": message
      });

      if (msg.status != 200) {
        raiseErrorToClient("Server rejected file.");
      }
    }
   catch (err) {
    raiseErrorToClient("Problem in fetch POST command.");
  }
}

/* set relevant uri to get flight of input flightId and call an ajax
 * get function.
 */
function getFlightObjectPromise(flightId) {
  let flightsPromise = getAllFlightsPromise();
  let flightPromise = flightsPromise.then(flights =>
                                           getFlightFromArray(flights,
                                                              flightId));

  return flightPromise;
}

// set relevant uri to get all flights and call an ajax get function.
function getAllFlightsPromise() {
  let currentTime = getCurrentTimeISO();
  let flightJson = null;
  let uri = GET_FLIGHT_URI + currentTime + "&sync_all";

  flightJson = doAjaxGet(uri)

  return flightJson;
}

// set relevant uri to get a flight plan and call an ajax get function.
function getFlightPlanPromise(flightId) {
  let flightPlan = null;
  let uri = GET_FLIGHT_PLAN_URI + flightId;    
    
  flightPlan = doAjaxGet(uri);

  return flightPlan;
}

// set relevant uri and return flight segments of flightId.
function getSegmentsPromise(flightId) {
    let flightPlanPromise = getFlightPlanPromise(flightId);
    let segments = null;


    flightPlanPromise.then(json => {return json.segments;});
}

function uploadFlightPlan(flightPlanString) {
 if (validateFlightPlanInput(flightPlanString)) {
    return answer = doAjaxPost(flightPlanString);
  } else {
   raiseErrorToClient("Invalid Json, couldn't upload.")
  }
}

async function removeFlightFromServer(flightId) {
  try {
    let msg = await fetch(DELETE_FLIGHT_URI+flightId, {
        "method": 'DELETE',
        "headers": HEADERS
      });

      if (msg.status != 200) {
        raiseErrorToClient("Deleting flight denied by server.");
      }
    }

   catch (err) {
    raiseErrorToClient("Network Error during fetch.");
  }
}

// get array of flights and call add flight on each entry.
function updateFlightsFromArray(flights) {
  if (flights) {
    if (flights.constructor !== Array) {
      flights = [flights];
    }

    for (flight of flights) {
      addFlight(flight);
    }
  }
}

// get flights from server and update in GUI and map.
function updateFlightsFromServer() {
  let serverFlightsPromise = getAllFlightsPromise();

  try {
    serverFlightsPromise.then(flights => updateFlightsFromArray(flights));
  } catch(err) {
    raiseErrorToClient("Could not get flights from server.");
  }
}
