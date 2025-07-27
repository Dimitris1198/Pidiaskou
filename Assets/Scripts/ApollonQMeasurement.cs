using System;

[Serializable]
public class ApollonQMeasurement
{
    public string created_at;
    public string alarm;
    public int master_value;
    public int tof_status;
    public int tof_distance;
    public int tof_index;
    public int master_value_filtered;
    public int radar_status;
    public int radar_peaks;
    public int radar_rd_1;
    public int radar_ra_1;
    public int radar_rd_2;
    public int radar_ra_2;
    public int radar_rd_3;
    public int radar_ra_3;
    public int radar_distance_max_peak;
    public int radar_amplitude_max_peak;
    public int acc_status;
    public int acc_orientation;
    public int acc_open;
    public int acc_impact;
    public int tof_nohist_stat;
    public int tof_nohist_dist;
    public int tof_hist_stat;
    public int tof_hist_dist;
    public int tof_peaks_idx;
    public int tof_peaks_num;
    public float target_value;
    public int rsrp;
    public int rsrq;
    public string metadata;
    public int acc_open_cnt;
    public float battery_voltage;
    public int internal_temperature;
    public string updated_at;
    public int sensor_id;
    public string formatted_log_date;

    public string ToCsvRow()
    {
        return string.Join(",", new string[]
        {
            created_at,
            alarm,
            master_value.ToString(),
            tof_status.ToString(),
            tof_distance.ToString(),
            tof_index.ToString(),
            master_value_filtered.ToString(),
            radar_status.ToString(),
            radar_peaks.ToString(),
            radar_rd_1.ToString(),
            radar_ra_1.ToString(),
            radar_rd_2.ToString(),
            radar_ra_2.ToString(),
            radar_rd_3.ToString(),
            radar_ra_3.ToString(),
            radar_distance_max_peak.ToString(),
            radar_amplitude_max_peak.ToString(),
            acc_status.ToString(),
            acc_orientation.ToString(),
            acc_open.ToString(),
            acc_impact.ToString(),
            tof_nohist_stat.ToString(),
            tof_nohist_dist.ToString(),
            tof_hist_stat.ToString(),
            tof_hist_dist.ToString(),
            tof_peaks_idx.ToString(),
            tof_peaks_num.ToString(),
            target_value.ToString("F3"),
            rsrp.ToString(),
            rsrq.ToString(),
            metadata,
            acc_open_cnt.ToString(),
            battery_voltage.ToString("F2"),
            internal_temperature.ToString(),
            updated_at,
            sensor_id.ToString(),
            formatted_log_date
        });
    }
}
