﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Triggers saving.
    /// </summary>
    public class SaveTrigger : Trigger
    {

        ///<inheritdoc/>
        protected override void TriggerAction(Transform other)
        {
            if (!Player.Instance)
                return;

            base.TriggerAction(other);

            SaveLoad.Instance().Save();
        }

    }
}