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
			this.Duration = 8f;
			this.Radius = 5f;
		}

		public IEnumerator Start()
		{
			this.HandleRadialIndicator();
			Instantiate(this.shadowPrefab, base.transform.position - base.GetComponent<tk2dBaseSprite>().GetRelativePositionFromAnchor(tk2dBaseSprite.Anchor.UpperCenter).WithX(0).ToVector3ZUp(0) + new Vector3(0f, 0.1875f), Quaternion.identity,
				this.transform);
			yield return new WaitForSeconds(this.Duration);
			Destroy(this.gameObject);
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
				float num = this.Radius;
				bool hasStat = this.statModifiers.ContainsKey(playerController);
				if (playerController && Vector2.Distance(playerController.CenterPosition, base.transform.position.XY()) < num)
				{
					if (!hasStat)
					{
						StatModifier mod = StatModifier.Create(PlayerStats.StatType.ReloadSpeed, StatModifier.ModifyMethod.ADDITIVE, -1000f);
						playerController.ownerlessStatModifiers.Add(mod);
						playerController.stats.RecalculateStats(playerController, true, false);
						this.statModifiers.Add(playerController, mod);
					}
				}
				else if (playerController)
				{
					if (hasStat)
					{
						StatModifier mod = this.statModifiers[playerController];
						if (playerController.ownerlessStatModifiers.Contains(mod))
						{
							playerController.ownerlessStatModifiers.Remove(mod);
							playerController.stats.RecalculateStats(playerController, true, false);
						}
						this.statModifiers.Remove(playerController);
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
					bool hasStat = this.statModifiers.ContainsKey(playerController);
					if (hasStat)
					{
						StatModifier mod = this.statModifiers[playerController];
						if (playerController.ownerlessStatModifiers.Contains(mod))
						{
							playerController.ownerlessStatModifiers.Remove(mod);
							playerController.stats.RecalculateStats(playerController, true, false);
						}
						this.statModifiers.Remove(playerController);
					}
				}
			}
			this.UnhandleRadialIndicator();
		}

		private void HandleRadialIndicator()
		{
			if (!this.m_radialIndicatorActive)
			{
				this.m_radialIndicatorActive = true;
				this.m_radialIndicator = ((GameObject)Instantiate(ResourceCache.Acquire("Global VFX/HeatIndicator"), base.transform.position, Quaternion.identity, base.transform)).GetComponent<HeatIndicatorController>();
				this.m_radialIndicator.CurrentColor = Color.white;
				this.m_radialIndicator.IsFire = false;
				float num = this.Radius;
				this.m_radialIndicator.CurrentRadius = num;
			}
		}

		private void UnhandleRadialIndicator()
		{
			if (this.m_radialIndicatorActive)
			{
				this.m_radialIndicatorActive = false;
				if (this.m_radialIndicator)
				{
					this.m_radialIndicator.EndEffect();
				}
				this.m_radialIndicator = null;
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
