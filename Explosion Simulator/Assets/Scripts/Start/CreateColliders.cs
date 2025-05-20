using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OpenCvSharp;
using UnityEngine.EventSystems;

namespace OpenCV2
{
    internal class Launch : MonoBehaviour, IPointerDownHandler
    {
        public RawImage ImageToPlace;
        public List<Texture2D> ColliderImages;
        public List<Texture2D> DisplayImages;
        public List<Texture2D> BackupImages;

        public List<string> ColliderFolder = new List<string>();
        public List<string> DisplayFolder = new List<string>();
        public List<string> BackupFolder = new List<string>();
        public List<LineRenderer> lineRenderers = new List<LineRenderer>();

        private Texture2D ColliderLayerTexture;

        public Mat InputMat;
        public GameObject ColliderPrefab;
        private List<Vector2> WhitePoints;
        public Transform ParentObject;

        private ExplosionPoint currentExplosion;
        [SerializeField]
        private GameObject ExplosionsParent;

        private bool collidersPlaced = false;
        private float sizedelta = 3f;
        private List<GameObject> currentColliders = new List<GameObject>();
        public GameObject TownList;

        private float lastClickTime = 0f;
        private float doubleClickThreshold = 0.3f;

        public int rows = 8;
        public int columns = 8;

        private bool layersSwapped = false;

        private int _threatRadius = 350;
        public int ThreatRadius
        {
            get => _threatRadius;
            set
            {
                if (value < 0)
                {
                    Debug.LogWarning("ThreatRadius не может быть меньше 0. Установлено значение 0.");
                    _threatRadius = 0;
                }
                else if (value > 10000)
                {
                    Debug.LogWarning("ThreatRadius не должен превышать 10000. Установлено 10000.");
                    _threatRadius = 10000;
                }
                else
                {
                    Debug.Log("ThreatRadius установлен на " + value);
                    _threatRadius = value;
                }
            }
        }

        private int _scatterRadius = 0;
        public int ScatterRadius
        {
            get => _scatterRadius;
            set
            {
                if (value < 0)
                {
                    Debug.LogWarning("ScatterRadius не может быть меньше 0. Установлено 0.");
                    _scatterRadius = 0;
                }
                else
                {
                    _scatterRadius = value;
                }
            }
        }

        private int _numberOfExplosions = 1;
        public int NumberOfExplosions
        {
            get => _numberOfExplosions;
            set
            {
                if (value < 1)
                {
                    Debug.LogWarning("NumberOfExplosions не может быть меньше 1. Установлено 1.");
                    _numberOfExplosions = 1;
                }
                else if (value > 40) // Можно задать ограничение
                {
                    Debug.LogWarning("NumberOfExplosions не должен превышать 10. Установлено 10.");
                    _numberOfExplosions = 40;
                }
                else
                {
                    _numberOfExplosions = value;
                }
            }
        }
        private void RemoveEachLine()
        {

            foreach (var line in lineRenderers)
            {
                Destroy(line);
            }


            lineRenderers.Clear();
        }
        private void Start()
        {
            collidersPlaced = false;

            ColliderImages = LoadTexturesFromFolder(ColliderFolder[0]);
            DisplayImages = LoadTexturesFromFolder(DisplayFolder[0]);
            BackupImages = LoadTexturesFromFolder(BackupFolder[0]);


            UpdateImageDisplay(DisplayImages);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            float currentClickTime = Time.time;

            if (currentClickTime - lastClickTime <= doubleClickThreshold)
            {
                HandleDoubleClick(eventData.position, NumberOfExplosions, ScatterRadius);
            }

            lastClickTime = currentClickTime;
        }

        private void HandleDoubleClick(Vector2 screenPosition, int numberOfExplosions, float scatterRadius)
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
            worldPosition.z = 0;
            RemoveOldColliders();

            List<Vector3> explosionPositions = GenerateExplosionPositions(worldPosition, numberOfExplosions, scatterRadius);
            HashSet<Vector3> colliderPositions = CalculateColliderPositions(explosionPositions, ThreatRadius);

            PlaceColliders(colliderPositions);
            TriggerExplosions(explosionPositions);
        }

        private List<Vector3> GenerateExplosionPositions(Vector3 initialPosition, int numberOfExplosions, float scatterRadius)
        {
            List<Vector3> explosionPositions = new List<Vector3> { initialPosition };

            for (int i = 1; i < numberOfExplosions; i++)
            {
                float randomAngle = UnityEngine.Random.Range(0f, Mathf.PI * 2);
                float randomDistance = UnityEngine.Random.Range(0f, scatterRadius);

                Vector3 scatterPosition = initialPosition + new Vector3(
                    Mathf.Cos(randomAngle) * randomDistance,
                    Mathf.Sin(randomAngle) * randomDistance,
                    0);

                explosionPositions.Add(scatterPosition);
            }

            return explosionPositions;
        }

        private HashSet<Vector3> CalculateColliderPositions(List<Vector3> explosionPositions, int explosionRadius)
        {
            HashSet<Vector3> colliderPositions = new HashSet<Vector3>();

            foreach (var explosionPosition in explosionPositions)
            {
                Mat colliderMat = MergeMatrices(ColliderImages);
                colliderMat.ConvertTo(colliderMat, MatType.CV_8UC3, 1.5f, 30);
                ImageProcessor.ApplyCanny(colliderMat, out WhitePoints);

                float imageWorldWidth = colliderMat.Width * sizedelta;
                float imageWorldHeight = colliderMat.Height * sizedelta;

                Vector3 bottomLeftWorldPosition = new Vector3
                (
                    transform.position.x - imageWorldWidth / 2,
                    transform.position.y - imageWorldHeight / 2,
                    0f
                );

                foreach (var point in WhitePoints)
                {
                    Vector3 position = new Vector3
                    (
                        bottomLeftWorldPosition.x + (point.x * sizedelta),
                        bottomLeftWorldPosition.y + (point.y * sizedelta),
                        0f
                    );

                    float distanceToExplosion = Vector3.Distance(position, explosionPosition);

                    if (distanceToExplosion <= explosionRadius)
                    {
                        colliderPositions.Add(position);
                    }
                }
            }

            return colliderPositions;
        }

        private void PlaceColliders(HashSet<Vector3> colliderPositions)
        {
            foreach (var position in colliderPositions)
            {
                GameObject colliderObject = Instantiate(ColliderPrefab, position, Quaternion.identity, ParentObject);

                BoxCollider boxCollider = colliderObject.GetComponent<BoxCollider>();
                if (boxCollider != null)
                    boxCollider.size = new Vector3(sizedelta, sizedelta, sizedelta);

                currentColliders.Add(colliderObject);
            }
        }

        private void TriggerExplosions(List<Vector3> explosionPositions)
        {
            foreach (var explosionPosition in explosionPositions)
            {
                var explosion = ExplosionPoint.CreatePoint(ExplosionsParent);
                explosion.Explode(new Vector2(explosionPosition.x, explosionPosition.y), ThreatRadius, ExplosionsParent);
                lineRenderers.AddRange(explosion.lineRenderers);
            }
        }

        private Mat MergeMatrices(List<Texture2D> images)
        {
            List<Mat> mats = new List<Mat>();

            foreach (var image in images)
            {
                mats.Add(ImageProcessor.Texture2DToMat(image));
            }

            List<List<Mat>> imageGroups = new List<List<Mat>>();
            int index = 0;

            for (int i = 0; i < rows; i++)
            {
                List<Mat> row = new List<Mat>();
                for (int j = 0; j < columns; j++)
                {
                    if (index < mats.Count)
                    {
                        row.Add(mats[index]);
                        index++;
                    }
                }
                imageGroups.Add(row);
            }

            Mat combinedMat = ImageProcessor.MergeImagesInZigzag(mats, rows, columns);
            return combinedMat;
        }

        private void RemoveOldColliders()
        {
            foreach (var collider in currentColliders)
            {
                Destroy(collider);
            }
            RemoveEachLine();

            currentColliders.Clear();
        }

        private void UpdateImageDisplay(List<Texture2D> imagesToDisplay)
        {
            Mat displayMat = MergeMatrices(imagesToDisplay);
            Texture2D displayTexture = ImageProcessor.MatToTextureBGR(displayMat);
            ImageToPlace.texture = displayTexture;
            ImageToPlace.GetComponent<RectTransform>().sizeDelta = new Vector2(displayTexture.width * sizedelta, displayTexture.height * sizedelta);
        }

        public List<Texture2D> LoadTexturesFromFolder(string folderName)
        {
            List<Texture2D> loadedTextures = new List<Texture2D>();

            Texture2D[] textures = Resources.LoadAll<Texture2D>(folderName);

            foreach (var texture in textures)
            {
                if (texture != null)
                {
                    loadedTextures.Add(texture);
                }
            }

            return loadedTextures;
        }
        private int currentLayerIndex = 0;
        public void ChangeTown()
        {
            // Перевірка, чи існує TownList
            if (TownList != null)
            {
                // Отримуємо компонент TMP_Dropdown
                TMP_Dropdown dropdown = TownList.GetComponent<TMP_Dropdown>();

                // Перевірка, чи знайдений компонент TMP_Dropdown
                if (dropdown != null)
                {
                    // Видаляємо старі колайдери
                    RemoveOldColliders();

                    // Отримуємо індекс вибраного міста
                    int townIndex = dropdown.value;

                    // Скидаємо поточний індекс шару
                    currentLayerIndex = 0;

                    // Завантажуємо зображення для вибраного міста, якщо індекс валідний
                    if (townIndex < ColliderFolder.Count && townIndex < DisplayFolder.Count && townIndex < BackupFolder.Count)
                    {
                        ColliderImages = LoadTexturesFromFolder(ColliderFolder[townIndex]);
                        DisplayImages = LoadTexturesFromFolder(DisplayFolder[townIndex]);
                        BackupImages = LoadTexturesFromFolder(BackupFolder[townIndex]);

                        // Оновлюємо відображення основного шару
                        UpdateImageDisplay(DisplayImages);
                    }
                }
            }
        }

        public void ToggleLayers()
        {
            currentLayerIndex = (currentLayerIndex + 1) % 2;

            switch (currentLayerIndex)
            {
                case 0:

                    UpdateImageDisplay(DisplayImages);
                    break;

                case 1:

                    UpdateImageDisplay(BackupImages);
                    break;
            }
        }
    }
}