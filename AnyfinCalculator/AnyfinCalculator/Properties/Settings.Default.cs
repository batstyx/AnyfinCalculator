namespace AnyfinCalculator.Properties
{
    public sealed partial class Settings
    {
        private int GetDefaultIntValue(string key) => int.Parse(Properties[key].DefaultValue.ToString());
        public int DefaultPlayerLeft => GetDefaultIntValue("Left");
        public int DefaultPlayerTop => GetDefaultIntValue("Top");

        public void ResetPosition()
        {
            Left = DefaultPlayerLeft;
            Top = DefaultPlayerTop;
        }

        private double GetDefaultDoubleValue(string key) => double.Parse(Properties[key].DefaultValue.ToString());
        public double DefaultPlayerOpacity => GetDefaultDoubleValue("Opacity");
        public double DefaultPlayerScale => GetDefaultDoubleValue("Scale");

        public void ResetDisplay()
        {
            Opacity = DefaultPlayerOpacity;
            Scale = DefaultPlayerScale;
        }
    }
}
