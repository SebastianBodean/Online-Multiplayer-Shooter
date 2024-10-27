using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : NetworkBehaviour
{
    private float healthBarLength = 200;

    private Image backgroundBar;

    private Image healthBar;

    private Image damageBar;

    private Coroutine damageCoroutine;

    private void Awake()
    {
        InitialiseDisplay();
    }

    //This sets the health display to the correct value for player who just joined before any value change has happened
    private void Start()
    {
        float health = GetComponent<SimpleHealth>().health.Value;
        float maxHealth = GetComponent<SimpleHealth>().maxHealth.Value;

        print(health);

        SetHealth(maxHealth, health, health);
    }

    /// <summary>
    /// GEnerates the health display above the entity
    /// </summary>
    private void InitialiseDisplay()
    {
        //Set up the canvas for the health display
        GameObject healthDisplay = new GameObject("Health Display");
        healthDisplay.transform.SetParent(transform);
        healthDisplay.AddComponent<Canvas>();
        healthDisplay.AddComponent<LookAtCamera>();
        //We display the healthbar above the entity based on its height
        healthDisplay.transform.localPosition = Vector3.up * (transform.GetComponent<Renderer>().bounds.size.y * 0.75f);

        //Set up the background of the health display
        GameObject newImage = new GameObject("Background Bar");
        backgroundBar = newImage.AddComponent<Image>();
        backgroundBar.rectTransform.SetParent(healthDisplay.transform);
        backgroundBar.color = new Color32(40, 40, 40, 255);
        backgroundBar.rectTransform.sizeDelta = new Vector2(healthBarLength, 10);

        //Set up the health bar
        newImage = new GameObject("Health Bar");
        healthBar = newImage.AddComponent<Image>();
        healthBar.rectTransform.SetParent(backgroundBar.transform);
        healthBar.color = new Color32(35, 124, 40, 255);
        healthBar.rectTransform.sizeDelta = new Vector2(0, 10);
        healthBar.rectTransform.anchorMax = new Vector2(0, 0.5f);
        healthBar.rectTransform.anchorMin = new Vector2(0, 0.5f);
        healthBar.rectTransform.pivot = new Vector2(0, 0.5f);

        //Set up the damage bar
        newImage = new GameObject("Damage Bar");
        damageBar = newImage.AddComponent<Image>();
        damageBar.rectTransform.SetParent(healthBar.transform);
        damageBar.color = new Color32(115, 43, 25, 255);
        damageBar.rectTransform.sizeDelta = new Vector2(0, 10);
        damageBar.rectTransform.anchorMax = new Vector2(1, 0.5f);
        damageBar.rectTransform.anchorMin = new Vector2(1, 0.5f);
        damageBar.rectTransform.pivot = new Vector2(0, 0.5f);
        damageBar.rectTransform.localPosition = Vector3.zero;

        //Scale everything down
        healthDisplay.transform.localScale = Vector2.one * 0.005f;
    }

    /// <summary>
    /// Updates the health display based on whether the entity gained or lost health
    /// </summary>
    /// <param name="maxHealth">The player's max health</param>
    /// <param name="newHealth">The player's health after the change</param>
    /// <param name="oldHealth">The player's health before the change</param>
    public void SetHealth(float maxHealth, float newHealth, float oldHealth)
    {
        float healthChange = newHealth - oldHealth;
        if (healthChange < 0)
            SetDamage(maxHealth, newHealth, -healthChange);
        else
            SetHeal(maxHealth, newHealth, healthChange);
    }

    /// <summary>
    /// Subtracts health from a player's health bar. Uses a damage bar to display the amount of health lost
    /// </summary>
    /// <param name="maxHealth"> The player's max health</param>
    /// <param name="health"> The player's health after the damage </param>
    /// <param name="damage"> The damage dealt to the player </param>
    private void SetDamage(float maxHealth, float health, float damage)
    {
        Vector2 dimensions;

        //Setting the size of the player's health bar
        dimensions = healthBar.rectTransform.sizeDelta;
        dimensions.x = (health / maxHealth) * healthBarLength;
        healthBar.rectTransform.sizeDelta = dimensions;

        //Increasing the damage bar
        dimensions = damageBar.rectTransform.sizeDelta;
        dimensions.x += (damage / maxHealth) * healthBarLength;
        damageBar.rectTransform.sizeDelta = dimensions;

        //If the player has taken damage already, we reset the coroutine
        if (damageCoroutine != null)
            StopCoroutine(damageCoroutine);
        damageCoroutine = StartCoroutine(DisplayDamage());
    }

    /// <summary>
    /// The reverse operation of SetDamage. If the player took damage in the past second, it will shrink the damage bar, otherwise it will leave it unchanged.
    /// </summary>
    /// <param name="maxHealth">The player's max health</param>
    /// <param name="health">The player's health after the heal </param>
    /// <param name="heal">The amount of health regained</param>
    private void SetHeal(float maxHealth, float health, float heal)
    {
        Vector2 dimensions;

        //Setting the size of the player's health bar
        dimensions = healthBar.rectTransform.sizeDelta;
        dimensions.x = (health / maxHealth) * healthBarLength;
        healthBar.rectTransform.sizeDelta = dimensions;

        if (damageBar.rectTransform.sizeDelta.x <= 0)
            return;

        //If the player took damage, decrease the damage bar
        dimensions = damageBar.rectTransform.sizeDelta;
        //We limit the dimensions so it never tries to set the damage bar to a negative value
        dimensions.x -= Mathf.Max((heal / maxHealth) * healthBarLength, 0);
        damageBar.rectTransform.sizeDelta = dimensions;
    }

    /// <summary>
    /// This coroutine displays the damage bar for one second after receiving damage, then shrinks the damage bar
    /// </summary>
    private IEnumerator DisplayDamage()
    {
        //Wait a second
        yield return new WaitForSeconds(1);

        //If the player hasn't taken any damage in the past second, we shrink the damage bar
        while (damageBar.rectTransform.sizeDelta.x > 0)
        {
            Vector2 dimensions = damageBar.rectTransform.sizeDelta;
            dimensions.x--;
            damageBar.rectTransform.sizeDelta = dimensions;

            yield return null;
        }

        //We set the width to 0 to prevent the bar from having a negative width
        float height = damageBar.rectTransform.sizeDelta.y;
        damageBar.rectTransform.sizeDelta = new Vector2(0, height);
    }
}
