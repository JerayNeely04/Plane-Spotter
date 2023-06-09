using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;

public class Flight
{
    public string ident;
    public string fa_flight_id;
    public Origin origin;
    public Destination destination;
    public LastPosition last_position;
    public string ident_prefix;
}

public class Origin
{
    public string code;
    public string name;
    public string city;
    public string airport_info_url;
}

public class Destination
{
    public string code;
    public string name;
    public string city;
    public string airport_info_url;
}

public class LastPosition
{
    public string fa_flight_id;
    public int altitude;
    public int groundspeed;
    public int heading;
    public double latitude;
    public double longitude;
}

public class Links
{
    public string next;
}

public class RootObject
{
    public Links links;
    public int num_pages;
    public List<Flight> flights;
}



public class FlightManager : MonoBehaviour
{
    public GameObject planeBaseObject;
    public GPS gps;
    public float flightSearchRadius;
    public int secondsPerUpdate;
    public string ApiKey;

    private List<Airplane> airplanesInScene = new List<Airplane>();
    private List<Flight> airplanesFromAPI;
    private long lastUpdatedMillis;

    // Start is called before the first frame update
    void Start()
    {
        lastUpdatedMillis = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        StartCoroutine(GetFlightsFromFA());
    }

    // Update is called once per frame
    void Update()
    {
        // if it's been enough seconds for the next update, update
        long currentTimeMillis = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        long millisElapsed = currentTimeMillis - lastUpdatedMillis;
        long millisPerUpdate = secondsPerUpdate * 1000;

        if (millisElapsed > millisPerUpdate)
        {
            lastUpdatedMillis = currentTimeMillis;

            // respawn the planes with updated positions
            StartCoroutine(GetFlightsFromFA());
        }
    }

    private void ClearExistingPlanes()
    {
        GameObject[] planes = GameObject.FindGameObjectsWithTag("Airplane");
        foreach (GameObject plane in planes)
        {
            Destroy(plane);
        }
    }

    IEnumerator GetFlightsFromFA()
    {
        double minLat = gps.getLatitude() - flightSearchRadius;
        double minLon = gps.getLongitude() - flightSearchRadius;
        double maxLat = gps.getLatitude() + flightSearchRadius;
        double maxLon = gps.getLongitude() + flightSearchRadius;
        using (UnityWebRequest request = UnityWebRequest.Get("https://aeroapi.flightaware.com/aeroapi/flights/search?" +
                    "query=-latlong+%22" +
                    minLat.ToString() + "+" +
                    minLon.ToString() + "+" +
                    maxLat.ToString() + "+" +
                    maxLon.ToString() + "%22"))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("x-apikey", ApiKey);
            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log(request.error);
            }
            else
            {
                Debug.Log("FLIGHTMANAGER: Successfully got text");
                var text = request.downloadHandler.text;
                // Assuming the JSON string is stored in a variable named jsonString
                Debug.Log("About to parse JSON");
                //RootObject rootObject = JsonConvert.DeserializeObject<RootObject>(text);
                RootObject rootObject = JsonConvert.DeserializeObject<RootObject>(text);
                Debug.Log("num planes: " + rootObject.flights.Count);
                airplanesFromAPI = rootObject.flights;

                ClearExistingPlanes();
                SpawnPlanes(airplanesFromAPI);

                yield return airplanesFromAPI;
            }
        }
    }

    public void SpawnPlanes(List<Flight> flights)
    {
        foreach (Flight plane in flights)
        {
            Debug.Log("Spawning plane: " + plane.ident);
            GameObject planeobject = Instantiate(planeBaseObject);

            Airplane ap = planeobject.GetComponent<Airplane>();
            ap.Elevation = plane.last_position.altitude;
            ap.Latitude = plane.last_position.latitude;
            ap.Longitude = plane.last_position.longitude;
            ap.Name = plane.origin.name;
            ap.Code = plane.origin.code;
            ap.PlaneId = plane.ident;
            ap.DestinationName = plane.destination.name;
            ap.DestinationCity = plane.destination.city;
            ap.Heading = plane.last_position.heading;
            ap.GroundSpeed = plane.last_position.groundspeed;

            ap.UpdateLastUpdateTime();
            planeobject.transform.rotation = Quaternion.Euler(0, 180 + plane.last_position.heading, 0);
            planeobject.SetActive(true);

            // for each plane that needs to be spawned, add it to our current list of airplanes
            airplanesInScene.Add(planeobject.GetComponent<Airplane>());
        }
    }

}


