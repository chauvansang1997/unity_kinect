using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Gamercial;
using CandyFactory.Events;

public class KinectClient : MonoBehaviour
{

    [SerializeField]
    private RawImage rawImage;

    [SerializeField]
    private RawImage point;

    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    GameObject blobPrefab;

    [SerializeField]
    float timeToClear = 1.0f;

    [SerializeField]
    Camera camera;

    [SerializeField]
    int port = 8080;

    float time = 0.0f;

    [SerializeField]
    float scale = 0.1f;

    bool _isCreate = false;


    private List<GameObject> blobs = new List<GameObject>();
    private UdpServer udpServer;
    // Start is called before the first frame update
    void Start()
    {
        if (camera == null)
        {
            camera = Camera.main;
        }
        udpServer = new UdpServer(port);
        udpServer.ServerMessage += UdpServer_ServerMessage;
        udpServer.Start();
    }
    private void UdpServer_ServerMessage(string message)
    {

        _isCreate = true;
        Debug.Log(message);
        try
        {
            foreach (var blob in blobs)
            {
                DestroyImmediate(blob);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        blobs.Clear();

        var json = JArray.Parse(message);
        foreach (var item in json)
        {
            time = 0;

            _isCreate = true;
            try
            {

                float x = item.Value<float>("x");
                float y = item.Value<float>("y");
                float size = item.Value<float>("size");
                //RawImage rawImage = Instantiate(rawImagePrefab);
                GameObject blob = Instantiate(blobPrefab);
                blob.transform.SetParent(rawImage.transform, false);

                Vector3 screenPosition = camera.WorldToScreenPoint(rawImage.rectTransform.transform.position);
                //  Vector3 test = Camera.main.WorldToScreenPoint(rawImage.rectTransform.transform.localPosition);


                x = rawImage.rectTransform.rect.width - x * rawImage.rectTransform.rect.width / 512;
                y = y * rawImage.rectTransform.rect.height / 424;

                Debug.Log("x:" + x);
                Debug.Log("y:" + y);
                Vector3 cameraPosition = new Vector3(x, y, camera.transform.position.z);

                //Vector3 rayDir = new Vector3(Screen.width / 2, Screen.height / 2);
                //Ray ray = camera.ScreenPointToRay(rayDir);

                blob.GetComponent<RectTransform>().anchoredPosition = new Vector2(x , y);
                Debug.Log("top:" + rawImage.rectTransform.rect.top);
                Debug.Log("bottom:" + rawImage.rectTransform.rect.bottom);
                Debug.Log("left:" + rawImage.rectTransform.rect.bottom);
                Debug.Log("right:" + rawImage.rectTransform.rect.bottom);
                Vector2 ray = new Vector2(rawImage.rectTransform.anchoredPosition.x + (512-item.Value<float>("x")) /512 * rawImage.rectTransform.sizeDelta.x,
                    rawImage.rectTransform.anchoredPosition.y + item.Value<float>("y")/424 * rawImage.rectTransform.sizeDelta.y);
                GameDirector.EventSystem.Emit(new EventTouchMoved(ray, Vector2.zero, Vector2.zero, camera));
                Debug.Log("ray:" + ray);
                cameraPosition = camera.ScreenToWorldPoint(cameraPosition);
                //cameraPosition = camera.ScreenToWorldPoint(cameraPosition);
                //Debug.Log("world:" + cameraPosition);
                //Rect worldRect = GetScreenCoordinates(rawImage.rectTransform);
                //float sphereSize = size * worldRect.width / 512;
                //RaycastHit[] sphereHits = Physics.SphereCastAll(cameraPosition, sphereSize, ray.direction, Mathf.Infinity);

                //Debug.DrawRay(cameraPosition, ray.direction, Color.red, 1.0f);
                //blob.transform.position = new Vector3(cameraPosition.x, cameraPosition.y, cameraPosition.z);
                //blob.transform.localScale = new Vector2(size * scale, size * scale);


                blobs.Add(blob);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }



        }


        _isCreate = false;
    }


    public Rect GetScreenCoordinates(RectTransform uiElement)
    {
        var worldCorners = new Vector3[4];
        uiElement.GetWorldCorners(worldCorners);
        var result = new Rect(
                      worldCorners[0].x,
                      worldCorners[0].y,
                      worldCorners[2].x - worldCorners[0].x,
                      worldCorners[2].y - worldCorners[0].y);
        return result;
    }
    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;

        if (time >= timeToClear)
        {
            if (!_isCreate)
            {
                try
                {
                    foreach (var blob in blobs)
                    {
                        DestroyImmediate(blob);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }

                blobs.Clear();
            }

            time = 0;
        }
    }

    private void OnDestroy()
    {
        if (udpServer != null)
        {
            udpServer.Disconnect();
        }
    }
}
