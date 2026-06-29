using EFT;

namespace BossSpawnControl
{
    internal static class BotFactionClassifier
    {
        /// <summary>
        /// PMC roles (pmcUSEC/pmcBEAR/pmcBot) must be checked before IsBossOrFollower —
        /// in EFT they are flagged as boss in BotSettingsRepo but count as USEC/BEAR for population limits.
        /// </summary>
        internal static BotFactionCategory Classify(WildSpawnType role, EPlayerSide side)
        {
            if (role.IsInfected())
            {
                return BotFactionCategory.Zombies;
            }

            if (role == WildSpawnType.pmcUSEC)
            {
                return BotFactionCategory.Usec;
            }

            if (role == WildSpawnType.pmcBEAR)
            {
                return BotFactionCategory.Bear;
            }

            if (role == WildSpawnType.pmcBot)
            {
                return side == EPlayerSide.Bear
                    ? BotFactionCategory.Bear
                    : BotFactionCategory.Usec;
            }

            if (role == WildSpawnType.shooterBTR)
            {
                return BotFactionCategory.BossesAndFollowers;
            }

            if (role.IsExUsec())
            {
                return BotFactionCategory.Rogues;
            }

            if (role.IsBossOrFollower())
            {
                return BotFactionCategory.BossesAndFollowers;
            }

            if (side == EPlayerSide.Bear)
            {
                return BotFactionCategory.Bear;
            }

            if (side == EPlayerSide.Usec)
            {
                return BotFactionCategory.Usec;
            }

            return BotFactionCategory.Scavs;
        }
    }
}
