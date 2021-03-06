﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class FollowingCamera : MonoBehaviour
{
    #region Private Fields

    [SerializeField]
    private new Camera camera;

    [SerializeField]
    private List<Transform> followedObjects;

    [SerializeField]
    private float minZoom = 8f, maxZoom = 15f;

    [SerializeField]
    private ZoomState state;

    [SerializeField]
    private bool visualizeArea;

    [SerializeField]
    [Range(0, 1)]
    private float zoomInBorders = 0.507f;

    [SerializeField]
    [Range(0, 1)]
    private float zoomOutBorders = 0.201f;

    [SerializeField]
    private float zoomSpeed = 0.2f;

    #endregion Private Fields

    #region Public Enums

    public enum ZoomState
    {
        Stay,
        ZoomIn,
        ZoomOut
    }

    #endregion Public Enums

    #region Private Methods

    private ZoomState CalculateZoomState()
    {
        ZoomState state = ZoomState.ZoomIn;
        float correctedZoomInBorders = zoomInBorders / 2f;
        float correctedZoomOutBorders = zoomOutBorders / 2f;
        for (int i = 0; i < followedObjects.Count; i++)
        {
            if (followedObjects[i] != null)
            {
                Vector3 viewPoint = camera.WorldToViewportPoint(followedObjects[i].position);

                Rect innerRect = new Rect(correctedZoomInBorders, correctedZoomInBorders, 1 - zoomInBorders, 1 - zoomInBorders);
                Rect outerRect = new Rect(correctedZoomOutBorders, correctedZoomOutBorders, 1 - zoomOutBorders, 1 - zoomOutBorders);

                if (!innerRect.Contains(viewPoint))
                    state = ZoomState.Stay;

                if (!outerRect.Contains(viewPoint))
                {
                    state = ZoomState.ZoomOut;
                    return state;
                }
            }
        }

        return state;
    }

    private void Awake()
    {
        if (camera == null)
            camera = GetComponentInChildren<Camera>();
    }

    private void Start()
    {
        camera.orthographicSize = minZoom;
    }

    private Vector3 FindCenterPoint()
    {
        if (followedObjects.Count == 0 || followedObjects[0] == null)
            return camera.transform.forward;

        Vector3 bottommost = followedObjects[0].position, leftmost = followedObjects[0].position, upmost = followedObjects[0].position, rightmost = followedObjects[0].position;
        for (int i = 1; i < followedObjects.Count; i++)
        {
            if (bottommost.x > followedObjects[i].position.x)
                bottommost = followedObjects[i].position;

            if (leftmost.z < followedObjects[i].position.z)
                leftmost = followedObjects[i].position;

            if (upmost.x < followedObjects[i].position.x)
                upmost = followedObjects[i].position;

            if (rightmost.z > followedObjects[i].position.z)
                rightmost = followedObjects[i].position;
        }

        return new Vector3(
            Mathf.Lerp(bottommost.x, upmost.x, 0.5f),
            Vector3.Lerp(
                Vector3.Lerp(leftmost, rightmost, 0.5f),
                Vector3.Lerp(upmost, bottommost, 0.5f),
                0.5f).y,
            Mathf.Lerp(leftmost.z, rightmost.z, 0.5f));
    }

    private void Move()
    {
        transform.position = FindCenterPoint();
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.red;
        //Gizmos.DrawSphere(transform.position, 0.3f);
    }

    private void OnGUI()
    {
        if (!visualizeArea)
            return;

        float correctedZoomInBorders = zoomInBorders / 2f;
        float correctedZoomOutBorders = zoomOutBorders / 2f;

        GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.2f);
        GUI.Box(
            new Rect(
                Screen.width * correctedZoomInBorders,
                Screen.height * correctedZoomInBorders,
                Screen.width - Screen.width * zoomInBorders,
                Screen.height - Screen.height * zoomInBorders
            ), "");
        GUI.Box(
            new Rect(
                Screen.width * correctedZoomOutBorders,
                Screen.height * correctedZoomOutBorders,
                Screen.width - Screen.width * zoomOutBorders,
                Screen.height - Screen.height * zoomOutBorders
            ), "");
    }

    private void Update()
    {
        for (int i = 0; i < followedObjects.Count; i++)
        {
            if (followedObjects[i] == null)
            {
                followedObjects.RemoveAt(i);
                i--;
            }
        }

        Move();
        Zoom();
    }

    public void AddToCamera(Transform player)
    {
        followedObjects.Add(player);
    }

    private void Zoom()
    {
        //   Vector3 position = camera.transform.localPosition;

        state = CalculateZoomState();

        switch (state)
        {
            case ZoomState.ZoomIn:
                camera.orthographicSize -= zoomSpeed;
                break;

            case ZoomState.ZoomOut:
                camera.orthographicSize += zoomSpeed;
                break;

            case ZoomState.Stay:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        if (camera.orthographicSize < minZoom)
            camera.orthographicSize = minZoom;

        if (camera.orthographicSize > maxZoom)
            camera.orthographicSize = maxZoom;
    }

    #endregion Private Methods
}