using UnityEngine;
using UnityEngine.UI;

public class UITextSelectedPart : MonoBehaviour
{
    private Text Text;

	private void Start ()
    {
        Text = GetComponent<Text>();
    }

    private void Update ()
    {
        if (DefaultBuilderBehaviour.Instance == null)
            return;

        if (DefaultBuilderBehaviour.Instance.SelectedPrefab == null)
            return;

        Text.text = "Current Part : " + DefaultBuilderBehaviour.Instance.SelectedPrefab.Name;
    }
}
