using UnityEngine;

public class EntityComponent : MonoBehaviour
{
    public StatManager statManager;
    public ResourceManager resourceManager;

    // For demo purposes
    private StatModifier testModifier;

    public EntityComponent()
    {
        statManager = new StatManager(this);
        resourceManager = new ResourceManager(this);
    }

    private void Start()
    {
        // Initialize stats
        statManager.AddStat(new Stat(StatType.Strength, 10));
        statManager.AddStat(new Stat(StatType.MaxHealth, 100));
        statManager.AddStat(new Stat(StatType.HealthRegen, 5));

        // Add listener for stat changes
        statManager.OnStatChanged += stat =>
        {
            Debug.Log($"Stat {stat.Type} updated: {stat.CurrentValue}");
        };
    }

    private void Update()
    {
        // Press Space to add a Strength→MaxHealth dependency modifier
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Remove previous test modifier if exists
            if (testModifier != null)
                statManager.RemoveModifier(testModifier);

            // Create a new dependency modifier
            testModifier = new StatDependencyModifier(
                StatType.MaxHealth,    // Target
                StatType.Strength,     // Source
                22f                    // Factor: each Strength adds 22 MaxHealth
            );

            statManager.AddModifier(testModifier);

            // Force recalculation
            float maxHealth = statManager.GetValue(StatType.MaxHealth);
            Debug.Log($"Applied modifier: MaxHealth = {maxHealth}");
        }

        // Optional: Press H to increase Strength base
        if (Input.GetKeyDown(KeyCode.H))
        {
            statManager.ChangeBaseValue(StatType.Strength, 1);
            Debug.Log($"Increased Strength: {statManager.GetValue(StatType.Strength)}");

        }
    }
}