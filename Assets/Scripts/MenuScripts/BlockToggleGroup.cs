﻿using UnityEngine;
using UnityEngine.UI;
using System.Linq;



using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Valve.VR.InteractionSystem;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    /// <summary>
    /// A component that represents a group of UI.Toggles.
    /// </summary>
    /// <remarks>
    /// When using a group reference the group from a UI.Toggle. Only one member of a group can be active at a time.
    /// </remarks>
    public class BlockToggleGroup : UIBehaviour
    {
        [SerializeField] private bool m_AllowSwitchOff = false;

        public delegate void ChangedEventHandler(BlockStructure blockColor);

        public event ChangedEventHandler OnChange;

        /// <summary>
        /// Is it allowed that no toggle is switched on?
        /// </summary>
        /// <remarks>
        /// If this setting is enabled, pressing the toggle that is currently switched on will switch it off, so that no toggle is switched on. If this setting is disabled, pressing the toggle that is currently switched on will not change its state.
        /// Note that even if allowSwitchOff is false, the Toggle Group will not enforce its constraint right away if no toggles in the group are switched on when the scene is loaded or when the group is instantiated. It will only prevent the user from switching a toggle off.
        /// </remarks>
        public bool allowSwitchOff { get { return m_AllowSwitchOff; } set { m_AllowSwitchOff = value; } }

        private List<BlockToggle> m_Toggles = new List<BlockToggle>();

        protected BlockToggleGroup()
        { }

        private void ValidateToggleIsInGroup(BlockToggle toggle)
        {
            if (toggle == null || !m_Toggles.Contains(toggle))
                throw new ArgumentException(string.Format("Toggle {0} is not part of ToggleGroup {1}", new object[] { toggle, this }));
        }

        public new void Start()
        {
            foreach (Transform transformToggle in gameObject.transform)
            {
                var toggle = transformToggle.gameObject.GetComponent<BlockToggle>();
                //var toggle = transformToggle.gameObject.GetComponents<Component>();
                toggle.onValueChanged.AddListener((isSelected, blockColor) =>
                {
                    if (!isSelected)
                    {
                        return;
                    }
                    var activeToggle = Active();
                    DoOnChange(blockColor);
                });
            }
        }

        public BlockToggle Active()
        {
            return ActiveToggles().FirstOrDefault();
        }

        protected virtual void DoOnChange(BlockStructure blockStructure)
        {
            ChangedEventHandler handler = OnChange;
            if (handler != null)
            {
                handler(blockStructure);
            }
        }

        /// <summary>
        /// Notify the group that the given toggle is enabled.
        /// </summary>
        /// <param name="toggle">The toggle that got triggered on</param>
        public void NotifyToggleOn(BlockToggle toggle, bool sendCallback = true)
        {
            ValidateToggleIsInGroup(toggle);
            // disable all toggles in the group
            for (var i = 0; i < m_Toggles.Count; i++)
            {
                if (m_Toggles[i] == toggle)
                    continue;

                if (sendCallback)
                    m_Toggles[i].isOn = false;
                else
                    m_Toggles[i].SetIsOnWithoutNotify(false);
            }
        }

        /// <summary>
        /// Unregister a toggle from the group.
        /// </summary>
        /// <param name="toggle">The toggle to remove.</param>
        public void UnregisterToggle(BlockToggle toggle)
        {
            if (m_Toggles.Contains(toggle))
                m_Toggles.Remove(toggle);

            if (!allowSwitchOff && !AnyTogglesOn() && m_Toggles.Count != 0)
            {
                m_Toggles[0].isOn = true;
                NotifyToggleOn(m_Toggles[0]);
            }
        }

        /// <summary>
        /// Register a toggle with the toggle group so it is watched for changes and notified if another toggle in the group changes.
        /// </summary>
        /// <param name="toggle">The toggle to register with the group.</param>
        public void RegisterToggle(BlockToggle toggle)
        {
            if (!m_Toggles.Contains(toggle))
                m_Toggles.Add(toggle);

            if (!allowSwitchOff && !AnyTogglesOn())
            {
                toggle.isOn = true;
                NotifyToggleOn(toggle);
            }
        }

        /// <summary>
        /// Are any of the toggles on?
        /// </summary>
        /// <returns>Are and of the toggles on?</returns>
        public bool AnyTogglesOn()
        {
            return m_Toggles.Find(x => x.isOn) != null;
        }

        /// <summary>
        /// Returns the toggles in this group that are active.
        /// </summary>
        /// <returns>The active toggles in the group.</returns>
        /// <remarks>
        /// Toggles belonging to this group but are not active either because their GameObject is inactive or because the Toggle component is disabled, are not returned as part of the list.
        /// </remarks>
        public IEnumerable<BlockToggle> ActiveToggles()
        {
            return m_Toggles.Where(x => x.isOn);
        }

        /// <summary>
        /// Switch all toggles off.
        /// </summary>
        /// <remarks>
        /// This method can be used to switch all toggles off, regardless of whether the allowSwitchOff property is enabled or not.
        /// </remarks>
        public void SetAllTogglesOff(bool sendCallback = true)
        {
            bool oldAllowSwitchOff = m_AllowSwitchOff;
            m_AllowSwitchOff = true;

            if (sendCallback)
            {
                for (var i = 0; i < m_Toggles.Count; i++)
                    m_Toggles[i].isOn = false;
            }
            else
            {
                for (var i = 0; i < m_Toggles.Count; i++)
                    m_Toggles[i].SetIsOnWithoutNotify(false);
            }

            m_AllowSwitchOff = oldAllowSwitchOff;
        }
    }
}

