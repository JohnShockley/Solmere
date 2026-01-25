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
    public EventBus Events { get; private set; }

    public GameObject CurrentTarget { get; set; }

    public EntityComponent()
    {


    }

    private void Awake()
    {
        Events = new EventBus();

        statManager = new StatManager(this, defaultStats);
        resourceManager = new ResourceManager(this, defaultResources);
        // Add listener for stat changes
        statManager.OnStatChanged += (type, value) =>
        {
            Debug.Log($"Generic listener: Stat {type} updated: {value}");
        };


        Events.OnDeath +=() =>
        {
            Debug.Log($"{gameObject.name} has died.");
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