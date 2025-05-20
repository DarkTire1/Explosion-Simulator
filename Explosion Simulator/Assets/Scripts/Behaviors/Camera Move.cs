using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class CameraMover : MonoBehaviour
{
    public float moveSpeed = 10f;
    public Camera cameraToMove;
    public RectTransform rawImageRect;

    public float zoomSpeed = 2f;
    public float minOrthographicSize;

    private Vector2 minBounds;
    private Vector2 maxBounds;
    private float cameraHeight;
    private float cameraWidth;

    private bool isDragging = false;
    private Vector3 lastMousePosition;
    public float sizeDifference = 3;

    private Texture2D handCursor;

    public Canvas uiCanvas;
    public TextMeshProUGUI zoomLevelText;

    public float leftMargin;
    public float rightMargin;
    public float topMargin;
    public float bottomMargin;

    private float scaleMultiplier = 1f;
    float zoomLevel = 50;

    void Start()
    {
        Screen.SetResolution(1024, 768, false);
        UpdateBounds();
        handCursor = Resources.Load<Texture2D>("HandCursor");
        UpdateZoomLevelText();
    }

    void Update()
    {
        UpdateBounds();

        if (IsPointerOverUI()) return;

        Vector3 cameraPosition = cameraToMove.transform.position;

        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
            Cursor.SetCursor(handCursor, Vector2.zero, CursorMode.Auto);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        if (isDragging)
        {
            Vector3 currentMousePosition = Input.mousePosition;
            Vector3 delta = cameraToMove.ScreenToWorldPoint(lastMousePosition) - cameraToMove.ScreenToWorldPoint(currentMousePosition);
            cameraPosition += delta;
            lastMousePosition = currentMousePosition;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            Vector3 mousePosition = Input.mousePosition;
            Vector3 worldMousePosition = cameraToMove.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, cameraToMove.transform.position.z));

            float newSize = cameraToMove.orthographicSize - scroll * zoomSpeed;
            newSize = Mathf.Clamp(newSize, minOrthographicSize, Mathf.Min((maxBounds.y - minBounds.y) / 2, (maxBounds.x - minBounds.x) / (2 * cameraToMove.aspect)));

            Vector3 beforeZoom = cameraToMove.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, cameraToMove.transform.position.z));
            cameraToMove.orthographicSize = newSize;
            Vector3 afterZoom = cameraToMove.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, cameraToMove.transform.position.z));

            Vector3 zoomOffset = beforeZoom - afterZoom;
            cameraPosition += zoomOffset;

            scaleMultiplier = cameraToMove.orthographicSize / minOrthographicSize / 2;

            UpdateZoomLevelText();
        }

        cameraHeight = 2f * cameraToMove.orthographicSize;
        cameraWidth = cameraHeight * cameraToMove.aspect;

        cameraPosition.x = Mathf.Clamp(cameraPosition.x, minBounds.x + cameraWidth / 2, maxBounds.x - cameraWidth / 2);
        cameraPosition.y = Mathf.Clamp(cameraPosition.y, minBounds.y + cameraHeight / 2, maxBounds.y - cameraHeight / 2);

        cameraToMove.transform.position = cameraPosition;
    }

    private void UpdateZoomLevelText()
    {
        if (zoomLevelText != null)
        {
            zoomLevel = Mathf.RoundToInt(50 * scaleMultiplier);
            zoomLevel = Mathf.Round(zoomLevel / 5f) * 5;
            zoomLevelText.text = $"{zoomLevel}";
        }
    }

    private void UpdateBounds()
    {
        Vector3[] worldCorners = new Vector3[4];
        rawImageRect.GetWorldCorners(worldCorners);

        minBounds = worldCorners[0];
        maxBounds = worldCorners[2];

        float leftMarginWorld = leftMargin / rawImageRect.lossyScale.x;
        float rightMarginWorld = rightMargin / rawImageRect.lossyScale.x;
        float topMarginWorld = topMargin / rawImageRect.lossyScale.y;
        float bottomMarginWorld = bottomMargin / rawImageRect.lossyScale.y;

        float scaleFactor = cameraToMove.orthographicSize / minOrthographicSize;

        minBounds.x -= (leftMarginWorld * sizeDifference) * scaleFactor;
        minBounds.y -= (bottomMarginWorld * sizeDifference) * scaleFactor;
        maxBounds.x += (rightMarginWorld * sizeDifference) * scaleFactor;
        maxBounds.y += (topMarginWorld * sizeDifference) * scaleFactor;

        cameraHeight = 2f * cameraToMove.orthographicSize;
        cameraWidth = cameraHeight * cameraToMove.aspect;
    }

    private bool IsPointerOverUI()
    {
        List<RaycastResult> raycastResults = new List<RaycastResult>();

        PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        foreach (var result in raycastResults)
        {
            RectTransform rectTransform = result.gameObject.GetComponent<RectTransform>();
            if (rectTransform != null && result.gameObject.transform.IsChildOf(uiCanvas.transform))
            {
                isDragging = false;
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (cameraToMove == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((minBounds + maxBounds) / 2, maxBounds - minBounds);
    }
}
