namespace Neurable.Interactions.Samples
{
    // NeurableUtilityControllable Objects have a distinct Elicitor function and can alter thier Intensity and Hue.
    public interface INeurableUtilityControllable
    {
        void RunElicitor();
        float ElicitorIntensity { get; set; }
        float ElicitorHue { get; set; }
    }
}
