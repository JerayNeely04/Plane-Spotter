using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using gps = GPS;

public class Airplane : MonoBehaviour
{
    public string PlaneId;
    public string Code;
    public string Name;
    public double Elevation;
    public double Longitude;
    public double Latitude;
    public String DestinationName;
    public String DestinationCity;

    public double Heading;
    public double GroundSpeed;
    public LineRenderer lineFront;
    public LineRenderer lineBack;
    public GPS gps;
    public TogglePlaneLines togglePlaneLines;

    private long lastUpdateTime;

    public void UpdateLastUpdateTime()
    {
        lastUpdateTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    }

    private float GetFeetTraveled()
    {
        long currentTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        long elapsed = currentTime - lastUpdateTime;

        // Convert the plane's knot speed to fps (feet per second)
        // Convert fps to fpms (feet per millisecond)
        float fps = (float)GroundSpeed * 1.68781f;
        float fpms = fps / 1000f;

        // Get number of feet traveled since last API update
        float traveled = fpms * elapsed;
        return traveled;
    }

    private void SetPosition()
    {
        PositionBindManager posManager = this.GetComponent<PositionBindManager>();
        Vector3 posNew = posManager.GetRawPosition(
            (float) Longitude,
            (float) Elevation,
            (float) Latitude,
            (float) gps.getLongitude(),
            (float) gps.getAltitude(),
            (float) gps.getLatitude()
        );

        // Add the feet traveled since the last API update
        posNew += transform.forward * GetFeetTraveled();
        posNew = posManager.NormalizePosition(posNew);
        posManager.SetBoundPosAndScale(this.gameObject, posNew);
    }

    public void UpdatePosition(float ElevationNew, float LatitudeNew, float LongitudeNew)
    {
        Longitude = LongitudeNew;
        Elevation = ElevationNew;
        Latitude = LatitudeNew;
    }

    private void UpdateLineToggle()
    {
        lineFront.enabled = togglePlaneLines.DisplayingLines();
        lineBack.enabled = togglePlaneLines.DisplayingLines();
    }

    private void InitLines()
    {
        // Initialize lines
        LineRenderer[] lines = { lineFront, lineBack };
        foreach (var line in lines)
        {
            line.positionCount = 2;
            line.useWorldSpace = false;
            line.startWidth = 0.3f;
            line.endWidth = 0.3f;
        }

        // Front
        lineFront.startColor = Color.blue;
        lineFront.endColor = Color.blue;
        lineFront.SetPosition(0, Vector3.zero);
        lineFront.SetPosition(1, Vector3.forward * 2000f);
        
        // Back
        lineBack.startColor = Color.green;
        lineBack.endColor = Color.green;
        lineBack.SetPosition(0, Vector3.zero);
        lineBack.SetPosition(1, Vector3.back * 2000f);

        // Start disabled
        lineFront.enabled = false;
        lineFront.enabled = false;
    }

    void Start()
    {
        InitLines();
    }

    // Update is called once per frame
    void Update()
    {
        SetPosition();
        UpdateLineToggle();
    }

}
