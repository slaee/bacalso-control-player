# HP and MP Control with fuzzy logic

![video](./Assets/bot-sample-on-run.gif)

## Running the script
To run this script you need to install the [Skua Bot](https://github.com/BrenoHenrike/Skua/releases) and setup the dependencies of it. Then clone this folder and copy as `BacalsoControlPlayer` to the `Scripts` folder of the Skua Bot.

After that, you can load and run the script like in the video shown above.

## How it works

The script uses the fuzzy logic to control the HP and MP of the player. The purpose of this is that the player will not die and can continue to fight. Note that this is good for support player only. If you are a DPS player, you should not use this script.

## Philosophy of the script

To make the terror long quest farm easier in the game.

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