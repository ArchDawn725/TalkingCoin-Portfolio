using UnityEngine;

public class DayCycle : MonoBehaviour
{
    [SerializeField] private Material[] skyboxes;
    [SerializeField] private Light dirLight;
    [SerializeField] private float dayDuration = 120f;// Duration of a full day in seconds
    [SerializeField] private float targetTime;
    [SerializeField] private int maxInteractions;
    public int interactions {  get; private set; }

    [SerializeField] private float cycleTime = 0f;
    [SerializeField] private bool isPaused = true;
    private Material blendedSkybox;

    void Start()
    {
        blendedSkybox = new Material(Shader.Find("Skybox/Blended"));
        UnityEngine.RenderSettings.skybox = blendedSkybox;
    }
    private void Update()
    {
        CheckTime();
        if (isPaused) return;

        cycleTime += Time.deltaTime;
        float timeProgress = cycleTime / dayDuration;

        // Handle the skybox changes
        //UpdateSkybox(timeProgress);

        // Handle the directional light (rotation, intensity, and color)
        UpdateDirectionalLight(timeProgress);

        // Reset cycle when a day ends
        if (cycleTime >= dayDuration)
        {
            cycleTime = 0f;
        }

        // Update global illumination
        DynamicGI.UpdateEnvironment();
    }

    // Method to update skybox based on time progress
    void UpdateSkybox(float timeProgress)
    {
        /*
        if (timeProgress < 0.25f) // Morning
        {
            RenderSettings.skybox = morningSkybox;
        }
        else if (timeProgress < 0.5f) // Afternoon
        {
            RenderSettings.skybox = afternoonSkybox;
        }
        else if (timeProgress < 0.75f) // Evening
        {
            RenderSettings.skybox = eveningSkybox;
        }
        else // Night
        {
            RenderSettings.skybox = nightSkybox;
        }
        */


        Material currentSkybox;
        Material nextSkybox;
        float blendFactor;

        // Determine current and next skybox, and blend factor
        if (timeProgress < 0.25f) // Morning to Afternoon
        {
            currentSkybox = skyboxes[0];
            nextSkybox = skyboxes[1];
            blendFactor = timeProgress / 0.25f;
        }
        else if (timeProgress < 0.5f) // Afternoon to Evening
        {
            currentSkybox = skyboxes[1];
            nextSkybox = skyboxes[2];
            blendFactor = (timeProgress - 0.25f) / 0.25f;
        }
        else if (timeProgress < 0.75f) // Evening to Night
        {
            currentSkybox = skyboxes[2];
            nextSkybox = skyboxes[3];
            blendFactor = (timeProgress - 0.5f) / 0.25f;
        }
        else // Night to Morning
        {
            currentSkybox = skyboxes[3];
            nextSkybox = skyboxes[0];
            blendFactor = (timeProgress - 0.75f) / 0.25f;
        }

        // Apply blend between current and next skybox
        blendedSkybox.SetTexture("_Tex", currentSkybox.GetTexture("_Tex"));
        blendedSkybox.SetTexture("_Tex2", nextSkybox.GetTexture("_Tex"));
        blendedSkybox.SetFloat("_Blend", blendFactor);

        // Reset cycle when a day ends
        if (cycleTime >= dayDuration)
        {
            cycleTime = 0f;
        }
    }

    // Method to update directional light properties
    void UpdateDirectionalLight(float timeProgress)
    {
        // Rotate the directional light (simulating the sun's movement)
        float rotationAngle = timeProgress * 235 - 20f; // -90 to start sunrise from the horizon
        dirLight.transform.rotation = Quaternion.Euler(rotationAngle, 180, 0f);

        // Adjust intensity and color based on the time of day
        if (timeProgress < 0.25f) // Morning
        {
            dirLight.intensity = Mathf.Lerp(0.2f, 1.0f, timeProgress / 0.25f); // Increase intensity
            dirLight.color = Color.Lerp(new Color(1.0f, 0.6f, 0.3f), Color.white, timeProgress / 0.25f); // Warm morning to white
        }
        else if (timeProgress < 0.5f) // Afternoon
        {
            dirLight.intensity = 1.0f; // Full brightness
            dirLight.color = Color.white; // Midday white light
        }
        else if (timeProgress < 0.75f) // Evening
        {
            float eveningProgress = (timeProgress - 0.5f) / 0.25f;
            dirLight.intensity = Mathf.Lerp(1.0f, 0.2f, eveningProgress); // Decrease intensity
            dirLight.color = Color.Lerp(Color.white, new Color(1.0f, 0.5f, 0.2f), eveningProgress); // White to warm evening light
        }
        else // Night
        {
            float nightProgress = (timeProgress - 0.75f) / 0.25f;
            dirLight.intensity = Mathf.Lerp(0.2f, 0.0f, nightProgress); // Fade out to night
            dirLight.color = Color.Lerp(new Color(1.0f, 0.5f, 0.2f), new Color(0.1f, 0.1f, 0.35f), nightProgress); // Warm to dark blue night
        }
    }

    private void CheckTime()
    {
        if (targetTime == 0) { cycleTime = 0; isPaused = false; return; }

        if (cycleTime >= targetTime) { isPaused = true; }
        else { isPaused = false; }
    }
    public void NextTime()
    {
        if (interactions < 5)
        {
            targetTime = (dayDuration / maxInteractions) * (interactions + 1);
        }
        else if (interactions == 5)
        {
            targetTime = dayDuration; // Set cycleTime to full day duration at the last interaction
        }
        interactions++;
    }
    public void NextDay()
    {
        interactions = 0;
        cycleTime = 0;
        isPaused = false;
        Controller.Instance.NewDay();
    }
}

