using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;
using Dungeonator;

namespace LichItems
{
    public class LichsBook : MonoBehaviour
    {
        public float Duration = 8f;
        public float Radius = 5f;
        private bool m_radialIndicatorActive;
        private HeatIndicatorController m_radialIndicator;
        public GameObject shadowPrefab;
        private readonly Dictionary<PlayerController, StatModifier> statModifiers = [];

        public IEnumerator Start()
        {
            HandleRadialIndicator();
            Instantiate(shadowPrefab, transform.position - GetComponent<tk2dBaseSprite>().GetRelativePositionFromAnchor(tk2dBaseSprite.Anchor.UpperCenter).WithX(0).ToVector3ZUp(0) + new Vector3(0f, 0.1875f), Quaternion.identity, transform);

            yield return new WaitForSeconds(Duration);

            Destroy(gameObject);
            yield break;
        }

        public void Update()
        {
            if (Dungeon.IsGenerating || GameManager.Instance.IsLoadingLevel)
                return;

            for (var i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
            {
                var player = GameManager.Instance.AllPlayers[i];

                if (player == null)
                    continue;

                var hasStat = statModifiers.ContainsKey(player);
                var inRange = Vector2.Distance(player.CenterPosition, transform.position.XY()) < Radius;

                if (!hasStat && inRange)
                {
                    var mod = StatModifier.Create(PlayerStats.StatType.ReloadSpeed, StatModifier.ModifyMethod.ADDITIVE, -1000f);
                    player.ownerlessStatModifiers.Add(mod);
                    player.stats.RecalculateStats(player, true, false);

                    statModifiers.Add(player, mod);
                }
                else if(hasStat && !inRange)
                {
                    var mod = statModifiers[player];

                    if (player.ownerlessStatModifiers.Contains(mod))
                    {
                        player.ownerlessStatModifiers.Remove(mod);
                        player.stats.RecalculateStats(player, true, false);
                    }

                    statModifiers.Remove(player);
                }
            }
        }

        public void OnDestroy()
        {
            for (var i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
            {
                var playerController = GameManager.Instance.AllPlayers[i];

                if (!playerController)
                    continue;

                if (!statModifiers.TryGetValue(playerController, out var mod))
                    continue;

                if (playerController.ownerlessStatModifiers.Contains(mod))
                {
                    playerController.ownerlessStatModifiers.Remove(mod);
                    playerController.stats.RecalculateStats(playerController, true, false);
                }

                statModifiers.Remove(playerController);
            }

            UnhandleRadialIndicator();
        }

        private void HandleRadialIndicator()
        {
            if (m_radialIndicatorActive)
                return;

            m_radialIndicatorActive = true;

            m_radialIndicator = (Instantiate(ResourceCache.Acquire("Global VFX/HeatIndicator"), transform.position, Quaternion.identity, transform) as GameObject).GetComponent<HeatIndicatorController>();
            m_radialIndicator.CurrentColor = Color.white;
            m_radialIndicator.IsFire = false;
            m_radialIndicator.CurrentRadius = Radius;
        }

        private void UnhandleRadialIndicator()
        {
            if (!m_radialIndicatorActive)
                return;

            m_radialIndicatorActive = false;
            if (m_radialIndicator)
                m_radialIndicator.EndEffect();

            m_radialIndicator = null;
        }
    }
}
