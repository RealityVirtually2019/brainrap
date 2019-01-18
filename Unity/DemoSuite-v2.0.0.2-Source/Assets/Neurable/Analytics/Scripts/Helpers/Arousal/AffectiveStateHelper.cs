namespace Neurable.Analytics
{
    public static class AffectiveStateHelper
    {
        public static AffectiveStateType GetEnumFromTitle(string title)
        {
            switch (title)
            {
                case "Attention":
                    return AffectiveStateType.Attention;
                case "Stress":
                    return AffectiveStateType.Stress;
                case "Calm":
                    return AffectiveStateType.Calm;
                case "Fatigue":
                    return AffectiveStateType.Fatigue;
                case "Grand Mean":
                    return AffectiveStateType.GrandMean;
            }

            return AffectiveStateType.None;
        }

        public static string GetTitleFromEnum(AffectiveStateType type)
        {
            switch (type)
            {
                case AffectiveStateType.Attention:
                    return "Attention";
                case AffectiveStateType.Stress:
                    return "Stress";
                case AffectiveStateType.Calm:
                    return "Calm";
                case AffectiveStateType.Fatigue:
                    return "Fatigue";
                case AffectiveStateType.GrandMean:
                    return "Grand Mean";
            }

            return "None";
        }
    }
}
