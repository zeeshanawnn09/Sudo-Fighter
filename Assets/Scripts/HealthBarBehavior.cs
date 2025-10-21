using UnityEngine;
using UnityEngine.UI;

public class HealthBarBehavior : MonoBehaviour
{
    public Slider healthbar;

    public void OnStartHealth(float health)
    {
        healthbar.maxValue = health;
        healthbar.value = health;
    }

    public void SetHealth(float health)
    {
        healthbar.value = health;
    }

}
