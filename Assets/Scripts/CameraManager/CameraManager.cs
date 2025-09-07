using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct CameraCanvasParam
{
    public int canvasID;
    public Canvas targetCanvas;
}

public class CameraManager : MonoBehaviour
{
    [SerializeField] private List<CameraCanvasParam> canvasList;
    [SerializeField] private float cameraMoveAmplitude = 5f;
    [SerializeField] private float duration = 1f;

    private Camera mainCamera;
    private CameraCanvasParam currentCanvas;

    private void Awake()
    {
        mainCamera = Camera.main;

        foreach (CameraCanvasParam canvasParam in canvasList)
        {
            if (canvasParam.targetCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                currentCanvas = canvasParam;
                break;
            }
        }
    }

    public void SetNewCanvas(int targetCanvasID)
    {
        if (targetCanvasID == currentCanvas.canvasID) return;

        CameraCanvasParam targetCanvas = GetCameraCanvasParamByCanvasID(targetCanvasID);

        currentCanvas.targetCanvas.renderMode = RenderMode.WorldSpace;

        Transform targetTransform = targetCanvas.targetCanvas.transform;

        float planeDistance = targetCanvas.targetCanvas.planeDistance;
        float distance = Vector3.Distance(currentCanvas.targetCanvas.transform.position, mainCamera.transform.position);

        Vector3 backPos = mainCamera.transform.position - mainCamera.transform.forward * cameraMoveAmplitude;

        Vector3 targetBackPos = targetTransform.position - targetTransform.forward * (cameraMoveAmplitude + distance);

        Vector3 finalPos = targetTransform.position - targetTransform.forward * planeDistance;

        Sequence seq = DOTween.Sequence();

        seq.Append(mainCamera.transform.DOMove(backPos, duration * 0.3f).SetEase(Ease.OutQuad));

        seq.Append(mainCamera.transform.DOMove(targetBackPos, duration * 0.5f).SetEase(Ease.InOutSine));
        seq.Join(mainCamera.transform.DORotateQuaternion(targetTransform.rotation, duration * 0.5f));

        seq.Append(mainCamera.transform.DOMove(finalPos, duration * 0.2f).SetEase(Ease.InQuad));

        seq.OnComplete(() =>
        {
            targetCanvas.targetCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            targetCanvas.targetCanvas.worldCamera = mainCamera;
            currentCanvas = targetCanvas;
        });
    }


    private CameraCanvasParam GetCameraCanvasParamByCanvasID(int canvasID)
    {
        return canvasList.FirstOrDefault(c => c.canvasID == canvasID);
    }
}
