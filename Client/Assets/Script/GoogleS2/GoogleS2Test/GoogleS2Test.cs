using System;
using System.Collections;
using System.Collections.Generic;
using Google.Common.Geometry;
using UnityEngine;

public class GoogleS2Test : MonoBehaviour {

    [SerializeField] private TextAsset m_Config;
    
    // Start is called before the first frame update
    private void Start() {
        var text = m_Config.text;
        var loopStrings = text.Split(';');
        List<S2Loop> loops = new List<S2Loop>();
        for (int i = 0; i < loopStrings.Length; i++) {
            List<S2Point> points = new List<S2Point>();
            var pointStrings = loopStrings[i].Split(',');
            for (int j = pointStrings.Length - 1; j >= 0; j--) {
                var valueStrings = pointStrings[j].Split(' ');
                var lat = double.Parse(valueStrings[1]);
                var lng = double.Parse(valueStrings[0]);
                double[] point = Gcj02ToWgs84(lng, lat);
                S2LatLng latLng = S2LatLng.FromDegrees(point[1], point[0]);
                points.Add(latLng.ToPoint());
            }
            loops.Add(new S2Loop(points));
        }
        
        S2Polygon polygon = new S2Polygon(loops);

        var test = Gcj02ToWgs84(39.8591547900,116.4221191400);
        
        Debug.Log(test[0]);
        Debug.Log(test[1]);
        
        Debug.Log(polygon.Contains(S2LatLng.FromDegrees(test[1],test[0]).ToPoint()));
    }

    double[] Gcj02ToWgs84(double lng, double lat) {
        
        double dlat = transformlat(lng - 105.0, lat - 35.0);
        double dlng = transformlon(lng - 105.0, lat - 35.0);
        double radlat = lat / 180.0 * Math.PI;
        double magic = Math.Sin(radlat);
        magic = 1 - 0.00669342162296594323 * magic * magic;
        double sqrtmagic = Math.Sqrt(magic);
        dlat = (dlat * 180.0) / ((6378245.0 * (1 - 0.00669342162296594323)) / (magic * sqrtmagic) * Math.PI);
        dlng = (dlng * 180.0) / (6378245.0 / sqrtmagic * Math.Cos(radlat) * Math.PI);
        double mglat = lat + dlat;
        double mglng = lng + dlng;
        return new double[]{lng * 2 - mglng, lat * 2 - mglat};
    }
    
    private double transformlon(double lon, double lat){
        double ret = 300.0 + lon + 2.0 * lat + 0.1 * lon * lon + 0.1 * lon * lat + 0.1 * Math.Sqrt(Math.Abs(lon));
        ret += (20.0 * Math.Sin(6.0 * lon * Math.PI) + 20.0 * Math.Sin(2.0 * lon * Math.PI)) * 2.0 / 3.0;
        ret += (20.0 * Math.Sin(lon * Math.PI) + 40.0 * Math.Sin(lon / 3.0 * Math.PI)) * 2.0 / 3.0;
        ret += (150.0 * Math.Sin(lon / 12.0 * Math.PI) + 300.0 * Math.Sin(lon / 30.0 * Math.PI)) * 2.0 / 3.0;
        return ret;
    }
    /**
     * 纬度转换
     * @param lon
     * @param lat
     * @return
     */
    private double transformlat(double lon, double lat){
        double ret = -100.0 + 2.0 * lon + 3.0 * lat + 0.2 * lat * lat + 0.1 * lon * lat + 0.2 * Math.Sqrt(Math.Abs(lon));
        ret += (20.0 * Math.Sin(6.0 * lon * Math.PI) + 20.0 * Math.Sin(2.0 * lon * Math.PI)) * 2.0 / 3.0;
        ret += (20.0 * Math.Sin(lat * Math.PI) + 40.0 * Math.Sin(lat / 3.0 * Math.PI)) * 2.0 / 3.0;
        ret += (160.0 * Math.Sin(lat / 12.0 * Math.PI) + 320 * Math.Sin(lat * Math.PI / 30.0)) * 2.0 / 3.0;
        return ret;
    }
}