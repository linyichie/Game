using System;
using System.Collections;
using System.Collections.Generic;
using Google.Common.Geometry;
using UnityEngine;

public class GoogleS2Test : MonoBehaviour {
    private S2Polygon polygon;

    private int index = 0;

    [SerializeField] private Transform current;
    [SerializeField] private Transform edge;

    [SerializeField] private TextAsset m_Config;

    [SerializeField] float duration = 1f;

    [SerializeField] private double[] lngLat;

    private float timer = 0f;

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

        polygon = new S2Polygon(loops);

        var test = Gcj02ToWgs84(lngLat[0], lngLat[1]);

        var testPoint = S2LatLng.FromDegrees(test[1], test[0]).ToPoint();

        Debug.Log(polygon.NumVertices);
        
        current.localPosition = new Vector3((float) testPoint.X * 10000, (float) testPoint.Y* 10000, (float) testPoint.Z* 10000);

        Debug.Log(polygon.Contains(testPoint));
    }

    private void Update() {
        if (timer > duration) {
            timer = 0f;

            index = index + 50;
            if (index >= polygon.NumVertices) {
                return;
            }

            var point = polygon.Loop(0).Vertex(index);
            var _edge = GameObject.Instantiate(edge);
            _edge.gameObject.SetActive(true);
            _edge.localPosition = new Vector3((float) point.X * 10000, (float) point.Y* 10000, (float) point.Z* 10000);
        }

        timer += Time.deltaTime;
    }

    double[] Gcj02ToWgs84(double lng, double lat) {
        if (out_of_china(lng, lat)) {
            return new double[] {lng, lat};
        }

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
        return new double[] {lng * 2 - mglng, lat * 2 - mglat};
    }

    private double transformlon(double lon, double lat) {
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
    private double transformlat(double lon, double lat) {
        double ret = -100.0 + 2.0 * lon + 3.0 * lat + 0.2 * lat * lat + 0.1 * lon * lat +
                     0.2 * Math.Sqrt(Math.Abs(lon));
        ret += (20.0 * Math.Sin(6.0 * lon * Math.PI) + 20.0 * Math.Sin(2.0 * lon * Math.PI)) * 2.0 / 3.0;
        ret += (20.0 * Math.Sin(lat * Math.PI) + 40.0 * Math.Sin(lat / 3.0 * Math.PI)) * 2.0 / 3.0;
        ret += (160.0 * Math.Sin(lat / 12.0 * Math.PI) + 320 * Math.Sin(lat * Math.PI / 30.0)) * 2.0 / 3.0;
        return ret;
    }

    private bool out_of_china(double lng, double lat) {
        if (lng < 72.004 || lng > 137.8347) {
            return true;
        }
        else if (lat < 0.8293 || lat > 55.8271) {
            return true;
        }

        return false;
    }
}