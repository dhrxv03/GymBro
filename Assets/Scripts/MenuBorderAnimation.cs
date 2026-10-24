using UnityEngine;

public class MenuBorderAnimation : MonoBehaviour
{
    [SerializeField] private RectTransform border;
    [SerializeField] private RectTransform[] dots;
    [SerializeField] private float speed = 140f;
    private float distance;

    private void Update()
    {
        float width = border.rect.width;
        float height = border.rect.height;
        float perimeter = 2f * (width + height);
        if (perimeter <= 0f || dots.Length == 0) return;
        distance = (distance + speed * Time.deltaTime) % perimeter;

        for (int i = 0; i < dots.Length; i++)
        {
            float position = (distance + i * perimeter / dots.Length) % perimeter;
            Vector2 point;
            if (position < width)
                point = new Vector2(position, height);
            else if (position < width + height)
                point = new Vector2(width, height - (position - width));
            else if (position < 2f * width + height)
                point = new Vector2(width - (position - width - height), 0f);
            else
                point = new Vector2(0f, position - 2f * width - height);
            dots[i].anchoredPosition = point - new Vector2(width, height) / 2f;
        }
    }
}
