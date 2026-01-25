using System;

public class EventBus
{
    // Damage pipeline
    public event Action<DamageEvent> OnPreDamageMitigation;
    public event Action<DamageEvent> OnDamageMitigation;
    public event Action<DamageEvent> OnPostDamageMitigation;
    public event Action<DamageEvent> OnDamageTaken;

    // Health
    public event Action<float> OnHealthChanged;
    public event Action OnDeath;

    // Resources
    //public event Action<ResourceEvent> OnResourceChanged;

    // Invoke wrappers — optional convenience
    public void RaisePreDamageMitigation(DamageEvent e) => OnPreDamageMitigation?.Invoke(e);
    public void RaiseDamageMitigation(DamageEvent e) => OnDamageMitigation?.Invoke(e);
    public void RaisePostDamageMitigation(DamageEvent e) => OnPostDamageMitigation?.Invoke(e);
    public void RaiseDamageTaken(DamageEvent e) => OnDamageTaken?.Invoke(e);

    public void RaiseHealthChanged(float value) => OnHealthChanged?.Invoke(value);
    public void RaiseDeath() => OnDeath?.Invoke();
}
