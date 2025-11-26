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

        if (Input.GetKeyDown(KeyCode.T))
        {
            DamageEffect damageEffect = EffectFactory.CreateDamageEffect(10f, StatType.Strength);
            EffectContext context = new(gameObject, statManager.Snapshot(), gameObject);
            damageEffect.Execute(context);
        }
    }
}