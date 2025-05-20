using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

internal class ExplosionPoint : MonoBehaviour
{
    private const int NUMBER_OF_RAYS = 800;
    private const float RAY_INTERVAL = 0f; // Інтервал між променями в секундах
    private const int MAX_ITERATIONS = 12; // Максимальна кількість ітерацій у ланцюжку

    public GameObject Parent;
    public float ThreatRadius; // Радіус загрози

    // Список для зберігання LineRenderer
    public List<LineRenderer> lineRenderers = new List<LineRenderer>();

    // Метод для створення нової точки вибуху
    public static ExplosionPoint CreatePoint(GameObject parent)
    {
        ExplosionPoint newPoint = new GameObject("Explosion Point").AddComponent<ExplosionPoint>();
        newPoint.transform.SetParent(parent.transform);
        return newPoint;
    }

    // Основний метод для запуску вибуху
    public void Explode(Vector2 position, float threatRadius, GameObject parent)
    {
        transform.SetParent(parent.transform);
        transform.position = position;
        ThreatRadius = threatRadius / 1.17370892018779f;

        CollideRays();
    }

    // Видалення старих ліній
    public void DeleteOldLines()
    {
        foreach (var line in lineRenderers)
        {
            if (line != null)
            {
                Destroy(line.gameObject);
            }
        }

        lineRenderers.Clear();
    }

    // Розрахунок напрямків для променів
    private Vector2[] CalculateDirections()
    {
        Vector2[] directions = new Vector2[NUMBER_OF_RAYS];
        for (int i = 0; i < NUMBER_OF_RAYS; i++)
        {
            float angle = i * (360f / NUMBER_OF_RAYS) * Mathf.Deg2Rad;
            directions[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
        return directions;
    }

    // Метод для обробки зіткнень
    private void ProcessCollisions(Collider2D initialCollider, int iteration, Color[] colors, List<Collider2D> collidersToDisable)
    {
        Queue<Collider2D> queue = new Queue<Collider2D>();
        queue.Enqueue(initialCollider);

        int currentIterations = 0; // Лічильник ітерацій
        HashSet<Collider2D> visitedColliders = new HashSet<Collider2D>();

        while (queue.Count > 0 && currentIterations < MAX_ITERATIONS)
        {
            Collider2D collider = queue.Dequeue();

            if (visitedColliders.Contains(collider))
                continue;

            visitedColliders.Add(collider);

            SpriteRenderer spriteRenderer = collider.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                int colorIndex = Mathf.Clamp((iteration - 1) / 2, 0, colors.Length - 1);
                Color newColor = colors[colorIndex];

                // Проверка текущего цвета коллайдера и изменение его при необходимости
                if (spriteRenderer.color == Color.clear || ShouldChangeColor(spriteRenderer.color, newColor))
                {
                    spriteRenderer.color = newColor;
                }
            }

            if (!collidersToDisable.Contains(collider))
                collidersToDisable.Add(collider);

            Collider2D[] touchingColliders = GetTouchingColliders(collider);

            foreach (Collider2D touchingCollider in touchingColliders)
            {
                if (touchingCollider != null && !collidersToDisable.Contains(touchingCollider))
                {
                    if (Vector2.Distance(collider.transform.position, touchingCollider.transform.position) < ThreatRadius)
                    {
                        queue.Enqueue(touchingCollider);
                    }
                }
            }

            currentIterations++;
        }
    }

    private bool ShouldChangeColor(Color currentColor, Color newColor)
    {
        // Приоритет цветов: красный > желтый > зеленый
        if (newColor == Color.red)
        {
            return true;
        }
        else if (newColor == Color.yellow)
        {
            return currentColor != Color.red && currentColor != Color.yellow;
        }
        else if (newColor == Color.green)
        {
            return currentColor == Color.clear;
        }
        return false;
    }

    // Основний метод для обробки променів
    private void CollideRays()
    {
        Vector2[] directions = CalculateDirections();
        Color[] colors = { new Color(1, 0, 0), new Color(1, 1, 0), new Color(0, 1, 0) };

        List<Collider2D> collidersToDisable = new List<Collider2D>();

        for (int i = 1; i <= 6; i++)
        {
            Color rayColor = colors[(i - 1) / 2 % colors.Length];

            for (int j = 0; j < NUMBER_OF_RAYS; j++)
            {
                Vector2 origin = (Vector2)transform.position;
                Vector2 direction = directions[j];

                RaycastHit2D hit = Physics2D.Raycast(origin, direction, ThreatRadius);

                if (hit.collider != null)
                {
                    if (i % 2 == 0)
                        DrawPermanentRay(origin, hit.point, rayColor, i);
                    ProcessCollisions(hit.collider, i, colors, collidersToDisable);
                }
                else
                {
                    Vector2 endPoint = origin + direction * ThreatRadius;
                    if (i % 2 == 0)
                        DrawPermanentRay(origin, endPoint, rayColor, i);
                }
            }

            foreach (var collider in collidersToDisable)
            {
                if (collider != null)
                    collider.enabled = false;
            }
        }

        // Включаем обратно все коллайдеры, которые были выключены
        foreach (var collider in collidersToDisable)
        {
            if (collider != null)
                collider.enabled = true;
        }
    }

    // Отримання всіх дотичних колайдерів
    private Collider2D[] GetTouchingColliders(Collider2D collider)
    {
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.useTriggers = false;
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(collider.gameObject.layer));
        List<Collider2D> results = new List<Collider2D>();
        collider.OverlapCollider(contactFilter, results);
        return results.ToArray();
    }

    // Малювання постійних променів
    private void DrawPermanentRay(Vector2 start, Vector2 end, Color color, int iteration)
    {
        GameObject lineObject = new GameObject("PermanentRay");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        lineObject.transform.position = new Vector3(0, 0, 3);
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.sortingLayerName = "Default";

        if (color.r == 1 && color.g == 0 && color.b == 0)
            lineRenderer.sortingOrder = 3;
        else if (color.r == 1 && color.g == 1 && color.b == 0)
            lineRenderer.sortingOrder = 2;
        else if (color.r == 0 && color.g == 1 && color.b == 0)
            lineRenderer.sortingOrder = 1;
        else
            lineRenderer.sortingOrder = 0;

        float distance = Vector2.Distance(start, end);
        float angleBetweenRays = (360f / NUMBER_OF_RAYS) * Mathf.Deg2Rad;
        float endWidth = 2 * distance * Mathf.Sin(angleBetweenRays / 2);

        lineRenderer.startWidth = 0f;
        lineRenderer.endWidth = endWidth;

        color.a = iteration == 2 ? 0.2f : (iteration == 4 ? 0.1f : 0.05f);
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        lineRenderer.SetPosition(0, new Vector3(start.x, start.y, 3));
        lineRenderer.SetPosition(1, new Vector3(end.x, end.y, 3));

        lineRenderers.Add(lineRenderer);
    }
}




