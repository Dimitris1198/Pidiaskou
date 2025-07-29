using System;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class ApollonQSensorSimulator : MonoBehaviour
{
    [Header("Sensor Settings")]
    public float maxRangeMM = 1400f;
    public float containerHeightMM = 1400f;
    public float fovDegrees = 40f;
    public int raysPerCircle = 256;
    public float radarNoiseStdDevMM = 20f;
    public float tofNoiseStdDevMM = 5f;
    public int sensorId = 106;
    public Transform binTransform;
    public Spawner spawner;

    public enum ClusteringMethods
    {
        NoCluster = 0, KMeans = 1, BiscectKMeans = 2, Birch = 3
    }

    [Header("Clustering method")]
    public ClusteringMethods clusterMethod;


    [Header("CSV Export")]
    public string csvFileName = "ApollonQ_Log.csv";

    private int? lastMasterValue = null;
    private List<Vector3> lastRayDirs = new List<Vector3>();

    // Previous valid radar readings
    private float prevRd1 = 0;
    private float prevRd2 = 0;
    private float prevRd3 = 0;
    private float prevRa1 = 0;
    private float prevRa2 = 0;
    private float prevRa3 = 0;


    void Start()
    {
        spawner = GameObject.Find("TrashSpanwer").GetComponent<Spawner>();
        binTransform = GameObject.Find("Bin").transform;

        string header = string.Join(",", new string[]
        {
            "created_at","alarm","master_value","tof_status","tof_distance","tof_index","master_value_filtered",
            "radar_status","radar_peaks","radar_rd_1","radar_ra_1","radar_rd_2","radar_ra_2",
            "radar_rd_3","radar_ra_3","radar_distance_max_peak","radar_amplitude_max_peak",
            "acc_status","acc_orientation","acc_open","acc_impact",
            "tof_nohist_stat","tof_nohist_dist","tof_hist_stat","tof_hist_dist",
            "tof_peaks_idx","tof_peaks_num",
            "target_value",
            "rsrp","rsrq","metadata",
            "acc_open_cnt","battery_voltage","internal_temperature",
            "updated_at","sensor_id","formatted_log_date"
        });

        if (!File.Exists(csvFileName))
            File.WriteAllText(csvFileName, header + Environment.NewLine);
    }

    public void GenerateAndSaveMeasurement()
    {
        ApollonQMeasurement m = PerformSensorScan();
        string row = m.ToCsvRow();
        File.AppendAllText(csvFileName, row + Environment.NewLine);
        Debug.Log("Measurement saved to CSV:\n" + row);
    }

    public ApollonQMeasurement PerformSensorScan()
    {
        lastRayDirs.Clear();

        List<float> hits = new List<float>();
        List<Vector3> clusteredHits = new List<Vector3>();

        // The main cone direction = 45� away from vertical
        Vector3 mainDirection = Quaternion.Euler(45, 0, 0) * -transform.up;
        int selectedOption = (int)clusterMethod;
        for (int i = 0; i < raysPerCircle; i++)
        {
            Vector3 dir = RandomConeDirection(mainDirection, fovDegrees);
            lastRayDirs.Add(dir);

            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, maxRangeMM / 1000f))
            {

                if (selectedOption > 0)
                {
                    clusteredHits.Add(hit.point);
                }
                else
                {
                    hits.Add(hit.distance * 1000f);
                }

            }
        }
        List<string> _clusterMethdos = new List<string>()
        {
            "clustering","birch_clustering","biscecting_clustering"
        };



        if (selectedOption > 0)
        {

            Debug.Log($"Sending data to python IPC (SNAKE)");
            Debug.Log("Selected method:" + _clusterMethdos[selectedOption - 1]);
            string jsonWrapped = JsonUtility.ToJson(new PositionsDTO(clusteredHits));

            IPC ipc = new IPC(_clusterMethdos[selectedOption - 1]);
            ipc.Start();
            ipc.Write(jsonWrapped);
            string res = ipc.Read();
            PeaksDTO output = JsonConvert.DeserializeObject<PeaksDTO>(res);
            ipc.Wait();
            Debug.Log("Receved data from python IPC (SNAKE)");
            var peaks = output.peaks;

            Vector3 rd1Vec = new Vector3(peaks[0][0], peaks[0][1], peaks[0][2]);
            Vector3 rd2Vec = new Vector3(peaks[1][0], peaks[1][1], peaks[1][2]);
            Vector3 rd3Vec = new Vector3(peaks[2][0], peaks[2][1], peaks[2][2]);

            Vector3 origin = this.transform.position;
            hits.Add(Vector3.Distance(rd1Vec, origin) * 1000f);
            hits.Add(Vector3.Distance(rd2Vec, origin) * 1000f);
            hits.Add(Vector3.Distance(rd3Vec, origin) * 1000f);

        }

        float tofDistance = hits.Count > 0 ? Average(hits) : maxRangeMM;
        tofDistance += SampleGaussian(0, tofNoiseStdDevMM);
        tofDistance = Mathf.Clamp(tofDistance, 0, maxRangeMM);

        int tofIndex;
        if (tofDistance <= 1300)
            tofIndex = 0;
        else if (tofDistance <= 3000)
            tofIndex = 1;
        else
            tofIndex = 2;

        int tofStatus = 0;
        if (UnityEngine.Random.value < 0.02f)
            tofStatus = 99;
        else if (tofDistance >= maxRangeMM)
            tofStatus = 2;

        int masterValue = Mathf.RoundToInt(tofDistance);
        masterValue = Mathf.Clamp(masterValue, 0, (int)containerHeightMM);

        string alarm = "NO ALARM";
        if (lastMasterValue.HasValue)
        {
            int diff = Mathf.Abs(masterValue - lastMasterValue.Value);
            if (diff >= 200)
                alarm = "ALARM";
        }
        lastMasterValue = masterValue;

        // NEW RADAR LOGIC
        int radarStatus = 1;
        int radarPeaks = 3; // max initially

        float rd1 = prevRd1;
        float rd2 = prevRd2;
        float rd3 = prevRd3;

        float ra1 = prevRa1;
        float ra2 = prevRa2;
        float ra3 = prevRa3;

        bool rd1Valid = UnityEngine.Random.value < 0.8f;
        bool rd2Valid = false;
        bool rd3Valid = UnityEngine.Random.value < 0.25f;

        // Randomize radar status
        if (UnityEngine.Random.value < 0.2f)
        {
            radarStatus = 0;
            radarPeaks = 0;
        }
        else
        {

            if (rd1Valid)
            {
                rd1 = hits.Count > 0 ? Mathf.Clamp(hits[0] + SampleGaussian(0, radarNoiseStdDevMM), 0, maxRangeMM) : prevRd1;
                ra1 = UnityEngine.Random.Range(-20f, 0f);
                prevRd1 = rd1;
                prevRa1 = ra1;
            }

            if (hits.Count > 1 && rd1Valid)
            {
                float candidateRd2 = Mathf.Clamp(hits[1] + SampleGaussian(0, radarNoiseStdDevMM), 0, maxRangeMM);
                if (Mathf.Abs(candidateRd2 - rd1) > 200)
                {
                    rd2Valid = UnityEngine.Random.value < 0.8f;
                    if (rd2Valid)
                    {
                        rd2 = candidateRd2;
                        ra2 = UnityEngine.Random.Range(-20f, 0f);
                        prevRd2 = rd2;
                        prevRa2 = ra2;
                    }
                }
            }

            if (!rd2Valid)
            {
                rd2 = prevRd2;
                ra2 = prevRa2;
            }

            if (hits.Count > 2 && rd3Valid)
            {
                rd3 = Mathf.Clamp(hits[2] + SampleGaussian(0, radarNoiseStdDevMM), 0, maxRangeMM);
                ra3 = UnityEngine.Random.Range(-20f, 0f);
                prevRd3 = rd3;
                prevRa3 = ra3;
            }
            else
            {
                rd3 = prevRd3;
                ra3 = prevRa3;
            }

            radarPeaks = 0;
            if (rd1Valid) radarPeaks++;
            if (rd2Valid) radarPeaks++;
            if (rd3Valid) radarPeaks++;
        }


        List<float> radarDistances = new List<float> { rd1, rd2, rd3 };
        List<float> radarAmplitudes = new List<float> { ra1, ra2, ra3 };

        int maxIdx = 0;
        float maxAmp = radarAmplitudes[0];

        for (int i = 1; i < radarAmplitudes.Count; i++)
        {
            if (radarAmplitudes[i] < maxAmp)
            {
                maxAmp = radarAmplitudes[i];
                maxIdx = i;
            }
        }

        float radarDistanceMaxPeak = radarDistances[maxIdx];
        float radarAmplitudeMaxPeak = radarAmplitudes[maxIdx];

        int masterValueFiltered = masterValue;

        int accStatus = 0;
        int accOrientation = 2;
        int accOpen = 0;
        int accImpact = 0;
        int accOpenCnt = 11;

        int tofNoHistStat = 2;
        float tofNoHistDist = tofDistance;

        int tofHistStat = 99;
        float tofHistDist = 0;

        int tofPeaksIdx = 0;
        int tofPeaksNum = radarPeaks;

        float fillFraction = CalculateFillLevel();

        int rsrp = -118 + UnityEngine.Random.Range(-2, 2);
        int rsrq = -18 + UnityEngine.Random.Range(-3, 3);

        float batteryVoltage = 5.9f + UnityEngine.Random.Range(-0.1f, 0.1f);
        int internalTemperature = 46 + UnityEngine.Random.Range(-2, 3);

        DateTime now = DateTime.Now;
        string createdAt = spawner.timeDisplay.text;
        string formattedLogDate = spawner.timeDisplay.text;
        string updatedAt = createdAt;

        var metadataDict = new Dictionary<string, string>
        {
            {"deviceId", "apollon_cellular_358299840277053"},
            {"latitude", "49.123"},
            {"longitude", "11.123"},
            {"timestamp", ((DateTimeOffset)now).ToUnixTimeMilliseconds().ToString()},
            {"deviceType", "apollon"},
            {"versionSub", "7"},
            {"versionMajor", "1"},
            {"versionMinor", "2"},
            {"customerTitle", "imagic"},
            {"deviceBaseType", "apollon"}
        };
        string metadataJson = JsonUtility.ToJson(new SerializableDict(metadataDict)).Replace("\"", "\\\"");

        return new ApollonQMeasurement
        {
            created_at = createdAt,
            alarm = alarm,
            master_value = masterValue,
            tof_status = tofStatus,
            tof_distance = Mathf.RoundToInt(tofDistance),
            tof_index = tofIndex,
            master_value_filtered = masterValueFiltered,
            radar_status = radarStatus,
            radar_peaks = radarPeaks,
            radar_rd_1 = Mathf.RoundToInt(rd1),
            radar_ra_1 = Mathf.RoundToInt(ra1),
            radar_rd_2 = Mathf.RoundToInt(rd2),
            radar_ra_2 = Mathf.RoundToInt(ra2),
            radar_rd_3 = Mathf.RoundToInt(rd3),
            radar_ra_3 = Mathf.RoundToInt(ra3),
            radar_distance_max_peak = Mathf.RoundToInt(radarDistanceMaxPeak),
            radar_amplitude_max_peak = Mathf.RoundToInt(radarAmplitudeMaxPeak),
            acc_status = accStatus,
            acc_orientation = accOrientation,
            acc_open = accOpen,
            acc_impact = accImpact,
            tof_nohist_stat = tofNoHistStat,
            tof_nohist_dist = Mathf.RoundToInt(tofNoHistDist),
            tof_hist_stat = tofHistStat,
            tof_hist_dist = Mathf.RoundToInt(tofHistDist),
            tof_peaks_idx = tofPeaksIdx,
            tof_peaks_num = tofPeaksNum,
            target_value = fillFraction,
            rsrp = rsrp,
            rsrq = rsrq,
            metadata = metadataJson,
            acc_open_cnt = accOpenCnt,
            battery_voltage = (float)Math.Round(batteryVoltage, 2),
            internal_temperature = internalTemperature,
            updated_at = updatedAt,
            sensor_id = sensorId,
            formatted_log_date = formattedLogDate
        };
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        if (lastRayDirs != null && lastRayDirs.Count > 0)
        {
            foreach (var dir in lastRayDirs)
            {
                Gizmos.DrawRay(transform.position, dir * (maxRangeMM / 1000f));
            }
        }
    }

    Vector3 RandomConeDirection(Vector3 coneDirection, float coneAngleDegrees)
    {
        float coneAngleRad = coneAngleDegrees * Mathf.Deg2Rad;

        float cosTheta = Mathf.Lerp(Mathf.Cos(coneAngleRad), 1f, UnityEngine.Random.value);
        float sinTheta = Mathf.Sqrt(1f - cosTheta * cosTheta);
        float phi = UnityEngine.Random.Range(0f, 2f * Mathf.PI);

        Vector3 localDir = new Vector3(
            sinTheta * Mathf.Cos(phi),
            sinTheta * Mathf.Sin(phi),
            cosTheta
        );

        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, coneDirection);
        return rotation * localDir;
    }

    float Average(List<float> values)
    {
        if (values.Count == 0) return maxRangeMM;
        float sum = 0;
        foreach (var v in values)
            sum += v;
        return sum / values.Count;
    }

    float SampleGaussian(float mean, float stdDev)
    {
        float u1 = 1f - UnityEngine.Random.value;
        float u2 = 1f - UnityEngine.Random.value;
        float randStdNormal = Mathf.Sqrt(-2f * Mathf.Log(u1)) *
                              Mathf.Sin(2f * Mathf.PI * u2);
        return mean + stdDev * randStdNormal;
    }

    [Serializable]
    public class SerializableDict
    {
        public List<string> keys = new List<string>();
        public List<string> values = new List<string>();

        public SerializableDict(Dictionary<string, string> dict)
        {
            foreach (var kvp in dict)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }
    }

    public float CalculateFillLevel()
    {
        if (binTransform == null)
        {
            Debug.LogWarning("Bin transform not assigned.");
            return 0f;
        }

        float binWidth = 1.1f;
        float binDepth = 0.65f;
        float binHeight = 1.0f;
        float wallThickness = 0.10f;

        int rayCount = 256;
        int gridRows = Mathf.CeilToInt(Mathf.Sqrt(rayCount));
        int gridCols = gridRows;

        float safeMargin = wallThickness + 0.005f;

        float xStart = -binWidth / 2 + safeMargin;
        float xEnd = +binWidth / 2 - safeMargin;

        float zStart = -binDepth / 2 + safeMargin;
        float zEnd = +binDepth / 2 - safeMargin;

        float yStart = binTransform.position.y + binHeight;

        float maxRayDistance = binHeight;
        float totalDistance = 0;

        List<Vector3> cords = new List<Vector3>();



        for (int i = 0; i < gridRows; i++)
        {
            for (int j = 0; j < gridCols; j++)
            {
                float u = (float)i / (gridRows - 1);
                float v = (float)j / (gridCols - 1);

                float x = Mathf.Lerp(xStart, xEnd, u);
                float z = Mathf.Lerp(zStart, zEnd, v);


                Vector3 local = new Vector3(x, 0f, z);
                Vector3 origin = binTransform.TransformPoint(local + new Vector3(0, binHeight, 0));

                Ray ray = new Ray(origin, Vector3.down);

                if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance))
                {
                    totalDistance += hit.distance;
                    cords.Add(hit.point);
                    Debug.DrawLine(origin, hit.point, Color.green, 0.5f);
                }
                else
                {
                    totalDistance += maxRayDistance;
                    Vector3 end = origin + Vector3.down * maxRayDistance;
                    Debug.DrawLine(origin, end, Color.red, 0.5f);
                }
            }
        }
        // string jsonWrapped = JsonUtility.ToJson(new PositionsDTO(cords));
        // IPC ipc = new IPC("neighbors");
        // ipc.Start();
        // ipc.Write(jsonWrapped);
        // string res = ipc.Read();

        // PeaksDTO output = JsonConvert.DeserializeObject<PeaksDTO>(res);

        // ipc.Wait();


        // ipc.End();
        float avgDistance = totalDistance / (gridRows * gridCols);
        float fillFraction = 1.0f - Mathf.Clamp01(avgDistance / binHeight);

        Debug.Log($"[ApollonQSensorSimulator] Avg hit distance: {avgDistance:F3}m | Fill: {fillFraction:F2}");
        return fillFraction;
    }
}
