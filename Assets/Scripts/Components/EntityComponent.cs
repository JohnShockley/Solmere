using UnityEngine;

public class EntityComponent : MonoBehaviour
{
    public StatManager statManager;
    public ResourceManager resourceManager;
    public DefaultStats defaultStats;
    public DefaultResources defaultResources;

    // For demo purposes
    private StatModifier testModifier;
    private RegenModifier regenModifier;

    public EntityComponent()
    {


    }

    private void Awake()
    {

        statManager = new StatManager(this, defaultStats);
        resourceManager = new ResourceManager(this, defaultResources);
        // Add listener for stat changes
        statManager.OnStatChanged += (type, value) =>
        {
            Debug.Log($"Generic listener: Stat {type} updated: {value}");
        };

    }

    private void Update()
    {
        resourceManager.Tick(Time.deltaTime);
        // Press Space to add a Strength→MaxHealth dependency modifier
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Remove previous test modifier if exists
            if (testModifier != null)
                statManager.RemoveModifier(testModifier);

            // Create a new dependency modifier
            testModifier = new StatDependencyModifier(
                StatType.HealthMax,    // Target
                StatType.Strength,     // Source
                22f                    // Factor: each Strength adds 22 MaxHealth
            );

            statManager.AddModifier(testModifier);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {

            // Create a new dependency modifier
            regenModifier = new RegenModifier(
               80,
                -8f
            );

            resourceManager.AddRegenModifier(ResourceType.Health, regenModifier);
            Debug.Log("Added negative regen modifier");
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
        
            // Create a new dependency modifier
            regenModifier = new RegenModifier(
               80,
                8f
            );

            resourceManager.AddRegenModifier(ResourceType.Health, regenModifier);
            Debug.Log("Added positive regen modifier");
        }

        // Optional: Press H to increase Strength base
        if (Input.GetKeyDown(KeyCode.H))
        {
            statManager.ChangeBaseValue(StatType.Strength, 1);
        }
    }
}