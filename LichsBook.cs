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
		public LichsBook()
		{
			Duration = 8f;
			Radius = 5f;
		}

		public IEnumerator Start()
		{
			HandleRadialIndicator();
			Instantiate(shadowPrefab, transform.position - GetComponent<tk2dBaseSprite>().GetRelativePositionFromAnchor(tk2dBaseSprite.Anchor.UpperCenter).WithX(0).ToVector3ZUp(0) + new Vector3(0f, 0.1875f), Quaternion.identity,
				transform);
			yield return new WaitForSeconds(Duration);
			Destroy(gameObject);
			yield break;
		}

		private void Update()
		{
			if (Dungeon.IsGenerating || GameManager.Instance.IsLoadingLevel)
			{
				return;
			}
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				PlayerController playerController = GameManager.Instance.AllPlayers[i];
				float num = Radius;
				bool hasStat = statModifiers.ContainsKey(playerController);
				if (playerController && Vector2.Distance(playerController.CenterPosition, transform.position.XY()) < num)
				{
					if (!hasStat)
					{
						StatModifier mod = StatModifier.Create(PlayerStats.StatType.ReloadSpeed, StatModifier.ModifyMethod.ADDITIVE, -1000f);
						playerController.ownerlessStatModifiers.Add(mod);
						playerController.stats.RecalculateStats(playerController, true, false);
						statModifiers.Add(playerController, mod);
					}
				}
				else if (playerController)
				{
					if (hasStat)
					{
						StatModifier mod = statModifiers[playerController];
						if (playerController.ownerlessStatModifiers.Contains(mod))
						{
							playerController.ownerlessStatModifiers.Remove(mod);
							playerController.stats.RecalculateStats(playerController, true, false);
						}
						statModifiers.Remove(playerController);
					}
				}
			}
		}

		private void OnDestroy()
		{
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				PlayerController playerController = GameManager.Instance.AllPlayers[i];
				if (playerController)
				{
					bool hasStat = statModifiers.ContainsKey(playerController);
					if (hasStat)
					{
						StatModifier mod = statModifiers[playerController];
						if (playerController.ownerlessStatModifiers.Contains(mod))
						{
							playerController.ownerlessStatModifiers.Remove(mod);
							playerController.stats.RecalculateStats(playerController, true, false);
						}
						statModifiers.Remove(playerController);
					}
				}
			}
			UnhandleRadialIndicator();
		}

		private void HandleRadialIndicator()
		{
			if (!m_radialIndicatorActive)
			{
				m_radialIndicatorActive = true;
				m_radialIndicator = ((GameObject)Instantiate(ResourceCache.Acquire("Global VFX/HeatIndicator"), base.transform.position, Quaternion.identity, base.transform)).GetComponent<HeatIndicatorController>();
				m_radialIndicator.CurrentColor = Color.white;
				m_radialIndicator.IsFire = false;
				float num = Radius;
				m_radialIndicator.CurrentRadius = num;
			}
		}

		private void UnhandleRadialIndicator()
		{
			if (m_radialIndicatorActive)
			{
				m_radialIndicatorActive = false;
				if (m_radialIndicator)
				{
					m_radialIndicator.EndEffect();
				}
				m_radialIndicator = null;
			}
		}

		public float Duration;
		public float Radius;
		private bool m_radialIndicatorActive;
		private HeatIndicatorController m_radialIndicator;
		public GameObject shadowPrefab;
		private Dictionary<PlayerController, StatModifier> statModifiers = new Dictionary<PlayerController, StatModifier>();
	}
}
