using UnityEngine;

public class BinBuilder : MonoBehaviour
{
    void Start()
    {
        float binWidth = 1.1f;         // meters
        float binDepth = 0.65f;        // meters
        float binHeight = 1.0f;        // meters
        float wallThickness = 0.02f;   // meters

        GameObject binParent = new GameObject("Bin");

        // Bottom
        GameObject bottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bottom.transform.parent = binParent.transform;
        bottom.transform.localScale = new Vector3(binWidth, wallThickness, binDepth);
        bottom.transform.localPosition = new Vector3(0, wallThickness / 2, 0);
 
        // Left Wall
        GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftWall.transform.parent = binParent.transform;
        leftWall.transform.localScale = new Vector3(wallThickness, binHeight, binDepth);
        leftWall.transform.localPosition = new Vector3(-(binWidth / 2 - wallThickness / 2), binHeight / 2 + wallThickness, 0);

        // Right Wall
        GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightWall.transform.parent = binParent.transform;
        rightWall.transform.localScale = new Vector3(wallThickness, binHeight, binDepth);
        rightWall.transform.localPosition = new Vector3(+(binWidth / 2 - wallThickness / 2), binHeight / 2 + wallThickness, 0);

        // Front Wall
        GameObject frontWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frontWall.transform.parent = binParent.transform;
        frontWall.transform.localScale = new Vector3(binWidth, binHeight, wallThickness);
        frontWall.transform.localPosition = new Vector3(0, binHeight / 2 + wallThickness, -(binDepth / 2 - wallThickness / 2));

        // Back Wall
        GameObject backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backWall.transform.parent = binParent.transform;
        backWall.transform.localScale = new Vector3(binWidth, binHeight, wallThickness);
        backWall.transform.localPosition = new Vector3(0, binHeight / 2 + wallThickness, +(binDepth / 2 - wallThickness / 2));

        // SENSOR
        float sensorWidth = 0.0548f;
        float sensorHeight = 0.0305f;
        float sensorDepth = 0.1098f;

        float sensorX = 0;
        float sensorY = binHeight - sensorHeight / 2;
        float sensorZ = (binDepth / 2) - wallThickness;

        GameObject sensor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sensor.name = "Sensor";
        sensor.transform.parent = binParent.transform;
        sensor.transform.localScale = new Vector3(sensorWidth, sensorHeight, sensorDepth);
        sensor.transform.localPosition = new Vector3(sensorX, sensorY, sensorZ);

   
        sensor.transform.localRotation = Quaternion.Euler(0, 90, 45);

        // SENSOR ORIGIN 
        GameObject sensorOrigin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sensorOrigin.name = "SensorOrigin";
        sensorOrigin.transform.parent = sensor.transform;

        // Scale it tiny so it looks like a "point"
        sensorOrigin.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
        sensorOrigin.transform.localPosition = Vector3.zero;
        sensorOrigin.transform.localRotation = Quaternion.Euler(0, 0, -45);

       
        sensorOrigin.AddComponent<ApollonQSensorSimulator>();

        Debug.Log("Bin, sensor, and sensor origin created!");

  

      
    
    }
}
