/*
 * KerbalGit - http://github.com/kria/KerbalGit
 * 
 * Copyright (C) 2015 Kristian Adrup
 * 
 * This file is part of KerbalGit.
 * 
 * KerbalGit is free software: you can redistribute it and/or modify it 
 * under the terms of the GNU General Public License as published by the 
 * Free Software Foundation, either version 3 of the License, or (at your
 * option) any later version. See included file COPYING for details.
 */

using System;
using UnityEngine;

namespace KerbalGit
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class EditorSaveHook : MonoBehaviour
    {
        public void Start()
        {
            try
            {
                EditorLogic.fetch.saveBtn.AddValueChangedDelegate(OnSaveButtonClick);
            }
            catch (Exception ex)
            {
                print("KerbalGit: EditorSaveHook Start exception: " + ex.Message);
            }
        }

        public void OnSaveButtonClick(IUIObject obj)
        {
            if (KerbalGitAddon.Instance != null)
                KerbalGitAddon.Instance.OnSaved(HighLogic.CurrentGame);
        }
    }
}
