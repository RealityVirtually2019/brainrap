namespace Neurable.Analytics
{
    public class PowerbandTypeHelper
    {
        public static PowerbandType GetEnumFromTitle(string title)
        {
            switch (title)
            {
                case "Alpha":
                    return PowerbandType.Alpha;
                case "Beta":
                    return PowerbandType.Beta;
                case "Gamma":
                    return PowerbandType.Gamma;
                case "Delta":
                    return PowerbandType.Delta;
                case "Theta":
                    return PowerbandType.Theta;
            }

            return PowerbandType.None;
        }

        public static string GetTitleFromEnum(PowerbandType type)
        {
            switch (type)
            {
                case PowerbandType.Alpha:
                    return "Alpha";
                case PowerbandType.Beta:
                    return "Beta";
                case PowerbandType.Gamma:
                    return "Gamma";
                case PowerbandType.Delta:
                    return "Delta";
                case PowerbandType.Theta:
                    return "Theta";
            }

            return "None";
        }
    }
}
