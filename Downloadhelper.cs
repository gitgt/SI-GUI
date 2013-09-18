﻿#region Licence
/*This file is part of the project "Reisisoft Server Install GUI",
 * which is licenced under LGPL v3+. You may find a copy in the source,
 * or obtain one at http://www.gnu.org/licenses/lgpl-3.0-standalone.html */
#endregion
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Xml.Serialization;
using System.IO;
/*
 * This files contains all data for downloading data or files
 */
namespace SI_GUI
{
    public partial class MainUI : Form
    {
        enum enum4DL_Special { LB, OB, T, M };
        void asyncdl_wrapper(enum4DL_Special version, bool helppack)
        {
            switch (version)
            {
                case (enum4DL_Special.LB):
                    startasyncdownload("http://download.documentfoundation.org/libreoffice/stable/", false, false, true, false, helppack);
                    break;
                case (enum4DL_Special.OB):
                    startasyncdownload("http://download.documentfoundation.org/libreoffice/stable/", false, false, false, true, helppack);
                    break;
                case (enum4DL_Special.T):
                    startasyncdownload("http://dev-builds.libreoffice.org/pre-releases/win/x86/?C=S;O=D", true, false, false, false, helppack);
                    break;
                case (enum4DL_Special.M):
                    if (Environment.OSVersion.Version.Major > 5) // Vista and newer
                        startasyncdownload("http://dev-builds.libreoffice.org/daily/master/Win-x86@39/current/", false, true, false, false);
                    else
                        startasyncdownload("http://dev-builds.libreoffice.org/daily/master/Win-x86@6-debug/current/", false, true, false, false); // XP and older
                    break;
            }
        }
        enum enum4DL_MoreDaily { TB6MasterDBG, TB39Master, TB09_41 }
        void asyncdl_wrapper(enum4DL_MoreDaily version)
        {
            switch (version)
            {
                case (enum4DL_MoreDaily.TB6MasterDBG):
                    startasyncdownload("http://dev-builds.libreoffice.org/daily/master/Win-x86@6-debug/current/", false, true, false, false);
                    break;
                case (enum4DL_MoreDaily.TB39Master):
                    startasyncdownload("http://dev-builds.libreoffice.org/daily/master/Win-x86@39/current/", false, true, false, false);
                    break;
                case (enum4DL_MoreDaily.TB09_41):
                    startasyncdownload("http://dev-builds.libreoffice.org/daily/libreoffice-4-1/Win-x86_9-Voreppe/current/", false, true, false, false);
                    break;
            }
        }

        // List of all web clients
        List<WebClient> list_wc = new List<WebClient>();
        #region Process for DL of main program
        Dictionary<int, long> dl_manager_current = new Dictionary<int, long>();
        Dictionary<int, long> dl_manager_total = new Dictionary<int, long>();
        Dictionary<int, bool> dl_is_hp = new Dictionary<int, bool>();
        float sum_current, sum_total;
        float percentage;

        void download_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            sum_current = 0;
            sum_total = 0;
            if (dl_manager_current.ContainsKey(sender.GetHashCode()))
            {
                // Known dl
                dl_manager_current.Remove(sender.GetHashCode());
            }
            else
            {
                // New download
                bool isHP = false;
                if (e.TotalBytesToReceive < 104857600)
                    isHP = true;
                dl_is_hp.Add(sender.GetHashCode(), isHP);
                dl_manager_total.Add(sender.GetHashCode(), e.TotalBytesToReceive);
            }
            // Set already downloaded bytes
            dl_manager_current.Add(sender.GetHashCode(), e.BytesReceived);
            foreach (KeyValuePair<int, long> kvp in dl_manager_current)
            {
                sum_current += kvp.Value;
            }
            foreach (KeyValuePair<int, long> kvp in dl_manager_total)
            {
                sum_total += kvp.Value;
            }
            try
            {
                percentage = 100f * sum_current / sum_total;
                progressBar.Value = (int)(percentage * 100);
                percent.Text = Math.Round(percentage, 2).ToString() + " %";
            }
            catch (Exception) { }
        }
        bool hp_or_installer; // hp if true
        Dictionary<int, string> dllocation = new Dictionary<int, string>();
        void download_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (!AdvancedFilenames)
                start_dl.Enabled = true;
            string filename;
            if (!e.Cancelled && dllocation.TryGetValue(sender.GetHashCode(), out filename))
            {
                give_message.ShowBalloonTip(10000, getstring("dl_finished_title"), getstring("dl_finished"), ToolTipIcon.Info);
                if (dl_is_hp.TryGetValue(sender.GetHashCode(), out hp_or_installer))
                {
                    if (!hp_or_installer) // Main installer
                        path_main.Text = filename;
                    else
                        path_help.Text = filename;

                    if (progressBar.Value == progressBar.Maximum)
                        resetDL();

                }
                else
                {
                    resetDL();
                }
            }
            else
                resetDL();
        }
        private void resetDL()
        {
            foreach (WebClient wc in list_wc)
                if (wc.IsBusy)
                    wc.CancelAsync();
            dl_manager_current.Clear();
            dl_manager_total.Clear();
            dl_is_hp.Clear();
            dllocation.Clear();
            list_wc.Clear();
            progressBar.Value = 0;
            percent.Text = "0 %";
        }
        #endregion


        #region Download of Master, Testing, Latest as well as Older branch
        public void startasyncdownload(string url, bool testing, bool master, bool latest_branch, bool older_branch)
        {
            startasyncdownload(url, testing, master, latest_branch, older_branch, false);
        }

        public void startasyncdownload(string url, bool testing, bool master, bool latest_branch, bool older_branch, bool helppack)
        {
            // Download
            bool cont = true;
            string[] version = new string[2];
            string lang = Convert.ToString(choose_lang.SelectedItem.ToString());
            try
            {
                if (helppack)
                {
                    if (lang == "")
                    {
                        cont = false;
                        throw new System.InvalidOperationException(getstring("error_langpack_nolang"));
                    }
                }
            }
            catch (Exception ex)
            {
                exceptionmessage(ex.Message);
            }
            if (cont)
            {
                string httpfile = downloadfile(url);
                if (httpfile != "error")
                {
                    if (master)
                    {
                        int starting_position;
                        for (int i = 0; i < 6; i++)
                        {
                            starting_position = httpfile.IndexOf("<a href=");
                            httpfile = httpfile.Remove(0, 9 + starting_position);
                        }
                        starting_position = httpfile.IndexOf(".msi");
                        httpfile = httpfile.Remove(starting_position + 4);
                    }
                    else if (testing)
                    {
                        int starting_position = httpfile.IndexOf("Lib");
                        url = "http://dev-builds.libreoffice.org/pre-releases/win/x86/";
                        httpfile = httpfile.Remove(0, starting_position);
                        starting_position = httpfile.IndexOf("msi") + 3;
                        httpfile = httpfile.Remove(starting_position);

                        if (helppack && (httpfile.Length != 2))
                        {
                            string vers2 = httpfile;
                            string insert = "_helppack_" + lang + ".msi";
                            starting_position = vers2.IndexOf("x86") + 3;
                            vers2 = vers2.Remove(starting_position);
                            vers2 += insert;
                            httpfile = vers2;
                        }

                    }
                    else if (latest_branch)
                    {
                        int i = httpfile.IndexOf("Metadata");
                        httpfile = httpfile.Remove(0, i);
                        for (int j = 0; j < 3; j++)
                        {
                            i = httpfile.IndexOf("href=");
                            httpfile = httpfile.Remove(0, i + 6);
                        }
                        httpfile = httpfile.Remove(5);
                        url = "http://download.documentfoundation.org/libreoffice/stable/" + httpfile + "/win/x86/";
                        if (helppack)
                            httpfile = "LibreOffice_" + httpfile + "_Win_x86_helppack_" + lang + ".msi";
                        else
                            httpfile = "LibreOffice_" + httpfile + "_Win_x86.msi";

                    }
                    else if (older_branch)
                    {
                        int i = httpfile.IndexOf(">Parent Directory<");
                        httpfile = httpfile.Remove(0, i);
                        i = httpfile.IndexOf("a href");
                        i += 8;
                        httpfile = httpfile.Remove(0, i);
                        i = httpfile.IndexOf("/");
                        httpfile = httpfile.Remove(i);
                        url = "http://download.documentfoundation.org/libreoffice/stable/" + httpfile + "/win/x86/";
                        if (helppack)
                            httpfile = "LibO_" + httpfile + "_Win_x86_helppack_" + lang + ".msi";
                        else
                            httpfile = "LibO_" + httpfile + "_Win_x86_install_multi.msi";
                    }

                    startDL(httpfile, url, master, helppack);

                }
            }
        }
        #endregion
        private WebClient getPreparedWebClient()
        {
            WebClient webc = new WebClient();
            webc.Headers["User-Agent"] = "LibreOffice Server Install Gui " + set.program_version();
            webc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(download_DownloadProgressChanged);
            webc.DownloadFileCompleted += new AsyncCompletedEventHandler(download_DownloadFileCompleted);
            return webc;
        }

        private void startDL(string programFilename, string finallink, bool master, bool helppack)
        {
            list_wc.Add(getPreparedWebClient());
            int wc = list_wc.Count - 1;
            string path = path_4_download;
            Uri uritofile = new Uri(finallink + programFilename);
            string originalFilename = programFilename;
            if (!AdvancedFilenames)
            {
                if (programFilename.Contains("msi"))
                {
                    if (helppack)
                        programFilename = "libo_hp.msi";
                    else
                        programFilename = "libo_installer.msi";
                }
                else
                {
                    if (helppack)
                        programFilename = "libo_hp.exe";
                    else
                        programFilename = "libo_installer.exe";
                }
                start_dl.Enabled = false;
            }
            path = Path.Combine(path, programFilename);
            string mb_question = getstring("versiondl");
            mb_question = mb_question.Replace("%version", originalFilename);

            // If filename is TY, then no testing build is available
            if (programFilename.Contains("exe") || programFilename.Contains("msi"))
            {
                if (MessageBox.Show(mb_question, getstring("startdl"), MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    // Send stats
                    if (!helppack)
                    {
                        piwik.sendDLstats(dl_versions.SelectedIndex, dl_versions.SelectedItem.ToString(), "");
                    }
                    if (!master && helppack)
                    {
                        piwik.sendDLstats(dl_versions.SelectedIndex, dl_versions.SelectedItem.ToString(), choose_lang.SelectedItem.ToString());
                    }
                    piwik.sendDLfilename(uritofile.ToString());
                    // End send stats
                    give_message.ShowBalloonTip(5000, getstring("dl_started_title"), getstring("dl_started"), ToolTipIcon.Info);
                    set_progressbar();
                    list_wc[wc].DownloadFileAsync(uritofile, path);
                    dllocation.Add(list_wc[wc].GetHashCode(), path);
                    int k = 0;
                    if (!master)
                    {
                        k = originalFilename.IndexOf("_") + 1;
                        originalFilename = originalFilename.Remove(0, k);
                        k = originalFilename.IndexOf("_");
                        originalFilename = originalFilename.Remove(k);
                    }
                    else
                    {
                        k = originalFilename.IndexOf("_");
                        originalFilename = originalFilename.Remove(k);
                    }
                    if (!helppack)
                        subfolder.Text = originalFilename;
                }
            }
            else
            {
                MessageBox.Show(getstring("notest_txt"), getstring("notest_ti"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void download_any_version(string LinktoFile, bool helppack)
        {
            // Get the final download links and initialize the download
            string httpfile = downloadfile(LinktoFile + "?C=S;O=D");
            int tmp = httpfile.IndexOf("Parent");
            httpfile = httpfile.Remove(0, tmp);
            tmp = httpfile.IndexOf("href") + 6;
            httpfile = httpfile.Remove(0, tmp);
            tmp = httpfile.IndexOf("\"");
            httpfile = httpfile.Remove(tmp);
            if (helppack)
            {
                // Helppack only

                //LibreOffice helppack
                if (httpfile.Contains("install_all"))
                {
                    // Old format
                    httpfile = httpfile.Replace("install_all_lang", "helppack_" + choose_lang.SelectedItem.ToString());
                }
                else if (httpfile.Contains("install_multi"))
                {
                    // Newer format
                    httpfile = httpfile.Replace("install_multi", "helppack_" + choose_lang.SelectedItem.ToString());
                }
                else
                {
                    // New format
                    httpfile = httpfile.Insert(httpfile.IndexOf("86") + 2, "_helppack_" + choose_lang.SelectedItem.ToString());
                }

                // Start download
            }
            startDL(httpfile, LinktoFile, false, helppack);
        }
        private string downloadfile(string url)
        {
            WebClient httpfileclient = new WebClient();
            Uri urlhttp = new Uri(url);
            string httpfile = "error";
            try
            {
                Stream httptextraw = httpfileclient.OpenRead(urlhttp);
                StreamReader httpreader = new StreamReader(httptextraw);
                httpfile = httpreader.ReadToEnd();
                httptextraw.Close();
                httpreader.Close();

            }
            catch (System.Net.WebException ex)
            {
                exceptionmessage(ex.Message);

            }
            return httpfile;
        }
        private string[] getLibO_List_of_DL()
        {
            string link = "http://downloadarchive.documentfoundation.org/libreoffice/old/";
            List<string> versions = new List<string>();
            string httpfile = httpfile = downloadfile(link), tmp;
            int i;
            bool goon = true;
            // Get the version numbers of LibreOffice
            i = httpfile.IndexOf("Details") + 7;
            httpfile = httpfile.Remove(0, i);
            i = httpfile.IndexOf("latest");
            httpfile = httpfile.Remove(i);
            while (goon)
            {
                try
                {
                    i = httpfile.IndexOf("href");
                    httpfile = httpfile.Remove(0, i);
                    i = httpfile.IndexOf(">") + 1;
                    httpfile = httpfile.Remove(0, i);
                    i = httpfile.IndexOf("<") - 1;
                    tmp = httpfile.Remove(i);
                    versions.Add(tmp);
                }
                catch (System.ArgumentOutOfRangeException)
                { goon = false; }
            }
            return versions.ToArray();
        }
        private string get_final_link(bool libo, string version)
        {
            if (libo)
                return "http://downloadarchive.documentfoundation.org/libreoffice/old/" + version + "/win/x86/";
            else
                return "???";
        }
        private void set_progressbar()
        {
            if (progressBar.Value == progressBar.Maximum || progressBar.Value == 0)
            {
                progressBar.Maximum = 10000;
                progressBar.Value = 0;
            }
        }
    }
}
