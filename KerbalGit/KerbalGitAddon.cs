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
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

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

        public static KerbalGitAddon Instance { get; private set; }

        void Awake()
        {
            if (Instance != null) return;
            Instance = this;
            DontDestroyOnLoad(this);
        }

        public void Start()
        {
            try
            {
                string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                print(String.Format("KerbalGit {0} started", version));
                modDir = String.Format("{0}GameData/{1}/", KSPUtil.ApplicationRootPath, ModName);
                savesDir = KSPUtil.ApplicationRootPath + "saves/";
                settings = ConfigNode.Load(modDir + "settings.cfg");
                wait = int.Parse(settings.GetValue("wait"));

                if (!Repository.IsValid(savesDir))
                {
                    print("KerbalGit: Initializing repo...");
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

        public void OnSaved(Game game)
        {
            StartCoroutine(DelayedOnSaved(game));
        }

        private IEnumerator DelayedOnSaved(Game game)
        {
            var timer = new Stopwatch();
            timer.Start();

            print("KerbalGit: Entering OnSaved");

            yield return new WaitForSeconds(1); // It seems ksp needs time to finish saving

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

                            var sb = new StringBuilder();
                            
                            sb.AppendFormat("Game: {0}", game.Title);
                            sb.AppendFormat(", Time: {0}", KSPUtil.PrintDate((int)game.UniversalTime, true, true));
                            if (Funding.Instance != null)
                                sb.AppendFormat(", Funds: {0:N0}", (int)Funding.Instance.Funds);
                            if (ResearchAndDevelopment.Instance != null)
                                sb.AppendFormat(", Science: {0:N0}", (int)ResearchAndDevelopment.Instance.Science);
                            if (Reputation.Instance != null)
                                sb.AppendFormat(", Reputation: {0}%", (int)Reputation.Instance.reputation / 10);
                            if (FlightGlobals.ready && FlightGlobals.Vessels != null)
                                sb.AppendFormat(", Flights: {0}", FlightGlobals.Vessels.Count(v => v.vesselType != VesselType.Debris && 
                                v.vesselType != VesselType.SpaceObject && v.vesselType != VesselType.Unknown));
                            if (Contracts.ContractSystem.Instance != null)
                                sb.AppendFormat(", Contracts: {0}", Contracts.ContractSystem.Instance.GetActiveContractCount());

                            print("KerbalGit: Committing...");
                            Commit commit = repo.Commit(sb.ToString(), author);
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
