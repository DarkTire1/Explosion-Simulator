using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropdownItemRightClick : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private TMP_Dropdown Dropdown;
    [SerializeField]
    private GameObject DeletePanel;
    [SerializeField]
    private string AmmoName;
    void Start()
    {
        Dropdown = GetComponentInParent<TMP_Dropdown>();
    }

    public void OnPointerClick(PointerEventData eventData) // Меняем параметр на PointerEventData
    {
        int Itemindex = transform.GetSiblingIndex();
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (Itemindex > 10)
            {
                Dropdown.GetComponent<DropdownManager>().IndexToDelete = Itemindex;
                Dropdown.Hide();
                DeletePanel.SetActive(true);
            }
        }
    }
}
