# HP and MP Control with fuzzy logic

![video](./Assets/bot-sample-on-run.gif)

## Running the script
To run this script you need to install the [Skua Bot](https://github.com/BrenoHenrike/Skua/releases) and setup the dependencies on it. Then clone this repo and copy as `BacalsoControlPlayer` to `Scripts` folder of Skua Bot.

After that, you can load and run the script like in the shown video above.

## How it works

The script uses the fuzzy logic to control the HP and MP of the player. The purpose of this is to protect the player bot not to die from monster's hit damage and can continue to fight. Note that this is good for support player only. If you are a DPS player, you can run this script but you need to have a class equipment that burst damage so while jumping in other room cell the monster is still burning with amount of damage from your burst class skills.

## Class Equipment Creators

The script will be used for HP and MP regeneration as based on the skills or passive skills of the class creation.

## Philosophy of the script

To make the terror long quest farm easier in the game.


### Implementation

```csharp
private void _KillMonsterForItem(string monster, string item, int quantity, bool log)
{
    if (log)
    {
        int dynamicQuantity = Bot.Inventory.GetQuantity(item);
        Log.Message($"Killing {monster} for {item}, ({dynamicQuantity}/{quantity}) [Inventory = {item}]");
    }
    
    double defuzzyVal = 0.0f;
    while (!Bot.ShouldExit && !CheckInventory(item, quantity))
    {
        if (!Bot.Combat.StopAttacking)
        {
            Bot.Combat.Attack(monster);
            FuzzifyValues();
            defuzzyVal = Defuzzy();
            if (defuzzyVal < 8.6)
            {
                // jump to room with no monster
                Map.JumpRoomCell("Enter", "Spawn");
                // then rest
                Bot.Player.Rest(true);
            }
            else if (defuzzyVal > 8 && Bot.Player.Cell == "Enter")
            {
                // resume killing the monster
                Map.JumpRoomCell("r2", "Bottom");
            }
        }
        Bot.Sleep(ActionDelay);
    }
}
 ```

### Membership Functions
```csharp
int fullHealth = Bot.Player.Health;
int halfHealth = (fullHealth/2);
Health.Add(new MembershipFunction("LOW", 0.0, 150, (halfHealth-(halfHealth/1.5)), (halfHealth-(halfHealth/2.5))));
Health.Add(new MembershipFunction("NORMAL", (halfHealth-(halfHealth/3)), (halfHealth-(halfHealth/6)), halfHealth, halfHealth+100));
Health.Add(new MembershipFunction("HIGH", halfHealth, (halfHealth+(halfHealth/6)), fullHealth, fullHealth));
_myHealth = new LinguisticVariable("HEALTH", Health);

Mana.Add(new MembershipFunction("LOW", 0.0, 0.0, 20.0, 30.0));
Mana.Add(new MembershipFunction("NORMAL", 30.0, 45.0, 50.0, 55.0));
Mana.Add(new MembershipFunction("HIGH", 50.0, 55.0, 100.0, 100.0));
_myMana = new LinguisticVariable("MANA", Mana);

PlayerCondition.Add(new MembershipFunction("REST_A_WHOLE_LOT", 0.0, 0.0, 1.0, 3.0));
PlayerCondition.Add(new MembershipFunction("REST_A_LOT", 2.5, 2.7, 2.8, 3.0));
PlayerCondition.Add(new MembershipFunction("REST_A_GOODAMT", 2.9, 3.1, 4.0, 4.5));
PlayerCondition.Add(new MembershipFunction("REST_A_LITTLE", 4.5, 5.0, 5.0, 6.0));
PlayerCondition.Add(new MembershipFunction("REST_A_VERYLITTLE", 6.0, 6.5, 7.0, 7.5));
PlayerCondition.Add(new MembershipFunction("DONTREST", 7.0, 7.5, 10, 10));
_myCondition = new LinguisticVariable("PLAYERCONDITION", PlayerCondition);
```

### Fuzzy Rules
```csharp
FuzzyRules.Add(new FuzzyRule("IF (HEALTH IS LOW) AND (MANA IS LOW) THEN PLAYERCONDITION IS REST_A_WHOLE_LOT"));
FuzzyRules.Add(new FuzzyRule("IF (HEALTH IS NORMAL) AND (MANA IS LOW) THEN PLAYERCONDITION IS REST_A_LOT"));
FuzzyRules.Add(new FuzzyRule("IF (HEALTH IS LOW) AND (MANA IS NORMAL) THEN PLAYERCONDITION IS REST_A_LOT"));
FuzzyRules.Add(new FuzzyRule("IF (HEALTH IS HIGH) AND (MANA IS LOW) THEN PLAYERCONDITION IS REST_A_GOODAMT"));
FuzzyRules.Add(new FuzzyRule("IF (HEALTH IS LOW) AND (MANA IS HIGH) THEN PLAYERCONDITION IS REST_A_GOODAMT"));
FuzzyRules.Add(new FuzzyRule("IF (HEALTH IS NORMAL) AND (MANA IS NORMAL) THEN PLAYERCONDITION IS REST_A_LITTLE"));
FuzzyRules.Add(new FuzzyRule("IF (HEALTH IS HIGH) AND (MANA IS NORMAL) THEN PLAYERCONDITION IS REST_A_VERYLITTLE"));
FuzzyRules.Add(new FuzzyRule("IF (HEALTH IS NORMAL) AND (MANA IS HIGH) THEN PLAYERCONDITION IS REST_A_VERYLITTLE"));
FuzzyRules.Add(new FuzzyRule("IF (HEALTH IS HIGH) AND (MANA IS HIGH) THEN PLAYERCONDITION IS DONTREST"));
FuzzyEngine.FuzzyRuleCollection = FuzzyRules;
```
