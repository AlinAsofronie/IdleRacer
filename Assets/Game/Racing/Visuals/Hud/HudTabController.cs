namespace IdleRacer.Racing.Visuals.Hud
{
    /// <summary>
    /// Pure presentation-state controller for the bottom tab bar. Default tab is Build.
    /// Does not touch race playback, domain state, or save data.
    /// </summary>
    public sealed class HudTabController
    {
        public HudTab ActiveTab { get; private set; } = HudTab.Build;

        public void Select(HudTab tab)
        {
            ActiveTab = tab;
        }

        public bool IsActive(HudTab tab) => ActiveTab == tab;
    }
}
