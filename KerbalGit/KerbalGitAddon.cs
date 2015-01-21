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

using LibGit2Sharp;
using System;
using System.Reflection;
using UnityEngine;
using System.IO;
using KerbalGit.Properties;
using System.Diagnostics;

namespace KerbalGit
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class KerbalGitAddon : MonoBehaviour
    {
        private const string ModName = "KerbalGit";
        ConfigNode settings;
        private int wait;
        private string savesDir;
        private string modDir;
        private DateTime latestCommit;

        public void Start()
        {
            try
            {
                string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                print(String.Format("KerbalGit {0} started", version));
                modDir = String.Format("{0}GameData/{1}/", KSPUtil.ApplicationRootPath, ModName);
                savesDir = KSPUtil.ApplicationRootPath + "saves/";
                settings = ConfigNode.Load(String.Format("{0}GameData/{1}/settings.cfg", KSPUtil.ApplicationRootPath, ModName));
                wait = int.Parse(settings.GetValue("wait"));

                if (!Repository.IsValid(savesDir))
                {
                    print("KerbalGit: Initializing repo");
                    Repository.Init(savesDir, modDir);
                    File.WriteAllText(savesDir + ".gitignore", Properties.Resources.gitignore);
                }

                GameEvents.onGameStateSaved.Add(OnSaved);
            }
            catch (Exception ex)
            {
                print("KerbalGit: Start exception: " + ex.Message);
            }
        }

        private void OnSaved(Game game)
        {
            var timer = new Stopwatch();
            timer.Start();

            print("KerbalGit: Entering OnSaved");

            try
            {
                if (latestCommit == DateTime.MinValue || (DateTime.Now - latestCommit) > TimeSpan.FromSeconds(wait))
                {
                    using (var repo = new Repository(savesDir))
                    {
                        var repoStatus = repo.Index.RetrieveStatus(new StatusOptions() { Show = StatusShowOption.WorkDirOnly });

                        bool isStaged = false;
                        foreach (var entry in repoStatus)
                        {
                            if (entry.State == FileStatus.Modified || entry.State == FileStatus.Untracked)
                            {
                                repo.Index.Stage(entry.FilePath);
                                isStaged = true;
                            }
                        }

                        if (isStaged)
                        {
                            Signature author = new Signature(settings.GetValue("name"), settings.GetValue("email"), DateTime.Now);

                            print("KerbalGit: Committing");
                            Commit commit = repo.Commit(settings.GetValue("message"), author);
                            latestCommit = DateTime.Now;
                        }
                    }
                } 
            }
            catch (Exception ex)
            {
                print("KerbalGit: OnSaved exception: " + ex.Message);
            }
            finally
            {
                timer.Stop();
                print("KerbalGit: Exiting OnSaved after " + timer.Elapsed);
            }
            
        }
    }
}
