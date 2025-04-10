using UnityEngine;

public class ExperienceComponent : MonoBehaviour
{
    [SerializeField] private int baseLevel = 1;
    [SerializeField] private int level = 1;
    [SerializeField] private int currentXP = 0;
    [SerializeField] private int xpToNextLevel = 240;

    public void GainExperience(int amount)
    {
        currentXP += amount;
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        if (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    // Handle leveling up
    private void LevelUp()
    {
        level++;

        xpToNextLevel += (level - 1) * 25 + 100;
        CheckLevelUp();

        Debug.Log($"{gameObject.name} leveled up! Current level: {level}, XP to next level: {xpToNextLevel}");
    }

    private void ResetLevel()
    { 
        level = baseLevel;
        currentXP = 0;
    }
}
