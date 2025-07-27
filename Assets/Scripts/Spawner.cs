using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{

    [Header("Cone Collision  Processon")]
    //public ConeCollisionProcessor cone_processor;
    public ApollonQSensorSimulator sensorSim;
    [Header("Prefabs")]
    public List<GameObject> fixedSizePrefabs;
    public List<GameObject> randomScalePrefabs;

    [Header("Spawn Timing")]
    public float minSpawnTime = 1f;
    public float maxSpawnTime = 3f;

    [Header("Random Scale Range")]
    public Vector3 minScale = new Vector3(0.5f, 0.5f, 0.5f);
    public Vector3 maxScale = new Vector3(0.52f, 0.52f, 0.52f);

    [Header("Spawn Area")]
    public Collider spawnArea;

    [Header("Time Simulation")]
    public float timeSpeed = 60f; // 60 = 1 second = 1 in-game minute
    private float simulatedTime = 0f; // in minutes

    private DateTime simulatedDate;

    [Header("Hourly Rush Multipliers (0-23)")]
    [Range(0f, 2f)] public float[] hourRush = new float[24];

    [Header("UI Time Display")]
    public Text timeDisplay;
    int previusHour;
    private bool cleanedToday = false;

    void Start()
    {
        // sensorSim = GameObject.Find("Bin").GetComponentInChildren<ApollonQSensorSimulator>();
        previusHour = Mathf.FloorToInt((simulatedTime / 60f) % 24);
        float gravityMultiplier = 4f;//120f; // 2x gravity speed
        Physics.gravity = new Vector3(0, -9.81f * gravityMultiplier, 0);
        if (timeDisplay != null)
        {
            timeDisplay.text = "HELLO TEST";
            timeDisplay.color = Color.white;
        }
        else
        {
            //       Debug.LogError("timeDisplay is not assigned!");
        }

        // Debug.Log("Spawner started.");

        if (hourRush.Length != 24)
        {
            //    Debug.LogWarning("HourRush array must have 24 entries. Resizing.");
            hourRush = new float[24];
        }

        simulatedDate = DateTime.Today;

        StartCoroutine(SpawnLoop());
    }

    void Update()
    {
        Debug.Log("Running");
        if (sensorSim == null) sensorSim = GameObject.Find("Bin").GetComponentInChildren<ApollonQSensorSimulator>();
        simulatedTime += Time.deltaTime * timeSpeed;

        int currentHour = Mathf.FloorToInt((simulatedTime / 60f) % 24);

        int currentMinute = Mathf.FloorToInt(simulatedTime % 60);

      
        if (simulatedTime >= 1440f)
        {
            simulatedTime -= 1440f;
            simulatedDate = simulatedDate.AddDays(1);
            cleanedToday = false; 
                           
        }

        string timeString = $"{currentHour:D2}:{currentMinute:D2}";
        string dateString = simulatedDate.ToString("yyyy-MM-dd");

        if (timeDisplay != null)
        {
            timeDisplay.text = dateString + " " + timeString;
        }
        if (currentHour > previusHour || currentHour == 0)
        {
            previusHour = currentHour;
            Debug.Log("Calling the scanner");

            // cone_processor.StartConeScan();
            sensorSim.GenerateAndSaveMeasurement();
        }
        // Trigger daily cleanup at 5:00 PM (17:00)
        //   if (currentHour == 5 && !cleanedToday)
        //  {
        //      CleanTrash();
        //      cleanedToday = true;
        //   }
        if (currentHour == 5 || currentHour ==  10 || currentHour == 15 || currentHour == 20 || currentHour == 0 )
        {
            CleanTrash();
            cleanedToday = true;
        }
    }

    IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(5);
        while (true)
        {
            int currentHour = Mathf.FloorToInt((simulatedTime / 60f) % 24);
            float rushMultiplier = hourRush[currentHour];

            float adjustedSpawnDelay = Random.Range(minSpawnTime, maxSpawnTime)
                                       / Mathf.Max(rushMultiplier, 0.01f);

       // Debug.Log($"[SPAWN LOOP] Hour={currentHour} RushMultiplier={rushMultiplier:F2}. Waiting {adjustedSpawnDelay:F2}s before next spawn.");

            yield return new WaitForSeconds(adjustedSpawnDelay);

            if (Random.value > rushMultiplier)
            {
             // Debug.Log($"[SPAWN SKIPPED] Current hour {currentHour} has low rush ({rushMultiplier:F2}). Skipping spawn this cycle.");
                continue;
            }

            SpawnRandomObject();
        }
    }

    void SpawnRandomObject()
    {
        GameObject prefab = null;
        bool useFixed = Random.value > 0.5f;

        if (useFixed && fixedSizePrefabs.Count > 0 )
        {
            prefab = fixedSizePrefabs[Random.Range(0, fixedSizePrefabs.Count)];
        }
        else if (randomScalePrefabs.Count > 0)
        {
            prefab = randomScalePrefabs[Random.Range(0, randomScalePrefabs.Count)];
        }
        else
        {
          Debug.LogWarning("[SPAWN FAILED] No prefabs to spawn.");
            return;
        }

        Vector3 spawnPosition = GetRandomPointInCollider(spawnArea);
        Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        GameObject obj = Instantiate(prefab, spawnPosition, randomRotation);

        // Assign the tag "Trash"
        obj.tag = "Trash";
        obj.layer = LayerMask.NameToLayer("Ground");
        obj.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        foreach (var t in obj.GetComponentsInChildren<Transform>(true))
        {
            t.gameObject.tag = "Trash";
            t.gameObject.layer = LayerMask.NameToLayer("Ground");
        }
    //    obj.AddComponent<MidAirCheckVisualizer>();
     //   obj.GetComponent<MidAirCheckVisualizer>().rayLength = cone_processor.downRayDistance;
       // obj.GetComponent<MidAirCheckVisualizer>().binTransform = cone_processor.binTransform;
        string spawnedName = prefab.name;

        if (!useFixed)
        {
            Vector3 scale = new Vector3(
                Random.Range(minScale.x, maxScale.x) * obj.transform.localScale.x,
                Random.Range(minScale.y, maxScale.y) * obj.transform.localScale.x,
                Random.Range(minScale.z, maxScale.z) * obj.transform.localScale.x
            );
       
            obj.transform.localScale = scale;
          
            // Debug.Log($"[SPAWNED] Random-scale prefab \"{spawnedName}\" at {spawnPosition} with scale {scale}");
        }
        else
        {
            // Debug.Log($"[SPAWNED] Fixed-size prefab \"{spawnedName}\" at {spawnPosition}");
        }
    }


    void CleanTrash()
    {
        GameObject[] trashObjects = GameObject.FindGameObjectsWithTag("Trash");
        int count = 0;

        foreach (GameObject obj in trashObjects)
        {
            Destroy(obj);
            count++;
        }

       // Debug.Log($"[CLEANUP] Deleted {count} 'trash'-tagged objects at 05:00 on {simulatedDate:yyyy-MM-dd}.");
    }

    Vector3 GetRandomPointInCollider(Collider col)
    {
        if (col is BoxCollider box)
        {
            Vector3 center = box.center + col.transform.position;
            Vector3 size = box.size * 0.5f;
            Vector3 randomPoint = center + new Vector3(
                Random.Range(-size.x, size.x),
                Random.Range(-size.y, size.y),
                Random.Range(-size.z, size.z)
            );

         //  // Debug.Log($"[RANDOM POINT] BoxCollider random point: {randomPoint}");
            return randomPoint;
        }
        else if (col is SphereCollider sphere)
        {
            Vector3 center = sphere.center + col.transform.position;
            Vector3 randomPoint = center + Random.insideUnitSphere * sphere.radius;

       //    // Debug.Log($"[RANDOM POINT] SphereCollider random point: {randomPoint}");
            return randomPoint;
        }
        else
        {
     //       Debug.LogWarning("[RANDOM POINT] Unsupported collider type. Returning collider position.");
            return col.transform.position;
        }
    }
}
