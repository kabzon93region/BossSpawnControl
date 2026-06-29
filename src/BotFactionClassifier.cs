using EFT;



namespace BossSpawnControl

{

    internal static class BotFactionClassifier

    {

        internal static BotFactionCategory Classify(WildSpawnType role, EPlayerSide side)

        {

            if (role.IsInfected())

            {

                return BotFactionCategory.Zombies;

            }



            if (role.IsBossOrFollower())

            {

                return BotFactionCategory.BossesAndFollowers;

            }



            if (role.IsExUsec())

            {

                return BotFactionCategory.Rogues;

            }



            if (role == WildSpawnType.pmcBEAR)

            {

                return BotFactionCategory.Bear;

            }



            if (role == WildSpawnType.pmcUSEC)

            {

                return BotFactionCategory.Usec;

            }



            if (role == WildSpawnType.pmcBot)

            {

                return side == EPlayerSide.Bear

                    ? BotFactionCategory.Bear

                    : BotFactionCategory.Usec;

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

