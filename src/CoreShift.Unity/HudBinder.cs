#if UNITY_2017_1_OR_NEWER
using CoreShift.Core;
using CoreShift.Core.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreShift.Unity
{
    /// <summary>
    /// Binds read-only <see cref="GameSnapshot"/> data to the in-game HUD.
    /// The simulation never reads from the UI, keeping the core engine-agnostic.
    /// </summary>
    public sealed class HudBinder : MonoBehaviour
    {
        [SerializeField] private CoreShiftRunner runner;
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider xpBar;
        [SerializeField] private TextMeshProUGUI waveLabel;
        [SerializeField] private TextMeshProUGUI levelLabel;

        private void Update()
        {
            if (runner is null || runner.World is null) return;

            GameSnapshot snapshot = runner.World.Snapshot();

            if (healthBar != null) healthBar.value = snapshot.HealthFraction;
            if (xpBar != null) xpBar.value = snapshot.XpFraction;
            if (waveLabel != null) waveLabel.text = "Wave " + snapshot.Wave;
            if (levelLabel != null) levelLabel.text = "Lv " + snapshot.Level;
        }
    }
}
#endif
