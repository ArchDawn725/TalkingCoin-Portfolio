using RootMotion;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController Instance;
    private void Awake() { Instance = this; }

    [SerializeField] private GameObject talking;
    [SerializeField] private GameObject moneyUIHolder;
    [SerializeField] private GameObject gender;

    [SerializeField] TextMeshProUGUI goldText;
    [SerializeField] TextMeshProUGUI objectiveText;
    [SerializeField] CameraControllerFPS fps;

    public void AssignPronoun(string value)
    {
        Controller.Instance.pronoun = value;
        gender.SetActive(false);
        moneyUIHolder.SetActive(true);
        talking.SetActive(true);

        Controller.Instance.StartUp();
        fps.enabled = true;
    }

    public void UpdateGold(int gold)
    {
        goldText.text = gold.ToString();
    }
    public void AssaignObjective(string objective)
    {
        objectiveText.text = "Objective: " + "\n" + objective.ToString();
    }
}
