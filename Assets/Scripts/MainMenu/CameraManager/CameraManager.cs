using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct CameraCanvasParam
{
    public int canvasID;
    public Vector3 targetPosition;
    public Vector3 targetRotation;
    public Canvas canvas;
}

public class CameraManager : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private List<CameraCanvasParam> canvasList;
    [SerializeField] private int startCanvasID;

    [Header("Move_Settings")]
    [SerializeField] private float cameraMoveAmplitude = 5f;
    [SerializeField] private float duration = 1f;

    [Header("Canvas_Settings")]
    [SerializeField] private Vector3 relativeCanvasScale;

    private Camera mainCamera;
    private CameraCanvasParam currentCanvas;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Start()
    {
        foreach (CameraCanvasParam canvasParam in canvasList)
        {
            if (canvasParam.canvasID == startCanvasID)
            {
                currentCanvas = canvasParam;
            }
            else
            {
                canvasParam.canvas.gameObject.SetActive(false);
            }
        }

        SetCorrectTransfrom(currentCanvas);

        float requiredDistance = GetRequiredCameraDistance(currentCanvas.canvas, mainCamera);
        mainCamera.transform.position = currentCanvas.targetPosition - currentCanvas.canvas.transform.forward * requiredDistance;
        mainCamera.transform.rotation = Quaternion.Euler(currentCanvas.targetRotation);

        currentCanvas.canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    }

    public void SetNewCanvas(int targetCanvasID)
    {
        if (targetCanvasID == currentCanvas.canvasID) return;

        CameraCanvasParam targetCanvas = GetCameraCanvasParamByCanvasID(targetCanvasID);

        SetCorrectTransfrom(currentCanvas);
        SetCorrectTransfrom(targetCanvas);

        Sequence seq = DOTween.Sequence();

        float distance = Vector3.Distance(mainCamera.transform.position, currentCanvas.canvas.transform.position);

        Vector3 firstPos = mainCamera.transform.position - mainCamera.transform.forward * cameraMoveAmplitude;
        Vector3 secondPos = targetCanvas.targetPosition - targetCanvas.canvas.transform.forward * (cameraMoveAmplitude + distance);
        float requiredDistance = GetRequiredCameraDistance(targetCanvas.canvas, mainCamera);
        Vector3 thirdPos = targetCanvas.targetPosition - targetCanvas.canvas.transform.forward * requiredDistance;

        seq.Append(mainCamera.transform.DOMove(firstPos, duration * 0.2f));

        seq.Append(mainCamera.transform.DOMove(secondPos, duration * 0.6f));
        seq.Join(mainCamera.transform.DORotate(targetCanvas.targetRotation, duration * 0.6f));

        seq.Append(mainCamera.transform.DOMove(thirdPos, duration * 0.2f));

        seq.OnComplete(() =>
        {
            targetCanvas.canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            targetCanvas.canvas.worldCamera = mainCamera;
            currentCanvas.canvas.gameObject.SetActive(false);
            currentCanvas = targetCanvas;
        });
    }

    private void SetCorrectTransfrom(CameraCanvasParam canvasParam)
    {
        canvasParam.canvas.renderMode = RenderMode.WorldSpace;
        canvasParam.canvas.transform.SetPositionAndRotation(canvasParam.targetPosition, Quaternion.Euler(canvasParam.targetRotation));
        canvasParam.canvas.transform.localScale = relativeCanvasScale;
        canvasParam.canvas.worldCamera = mainCamera;
        canvasParam.canvas.gameObject.SetActive(true);
    }

    private float GetRequiredCameraDistance(Canvas canvas, Camera cam)
    {
        RectTransform rt = canvas.GetComponent<RectTransform>();

        Vector2 size = Vector2.Scale(rt.rect.size, rt.lossyScale);

        float halfHeight = size.y / 2f;

        float halfFovRad = cam.fieldOfView * 0.5f * Mathf.Deg2Rad;

        float requiredDistance = halfHeight / Mathf.Tan(halfFovRad);

        return requiredDistance;
    }


    private CameraCanvasParam GetCameraCanvasParamByCanvasID(int canvasID)
    {
        return canvasList.FirstOrDefault(c => c.canvasID == canvasID);
    }
}
