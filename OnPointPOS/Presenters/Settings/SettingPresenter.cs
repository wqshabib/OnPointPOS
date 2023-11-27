using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http;
using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Utils;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Configuration;
using System.Net;
using System.Text;
using System.Windows.Documents;
using POSSUM.ApiModel;
using POSSUM.Res;

namespace POSSUM.Presenters.Settings
{
    public class SettingPresenter
    {
        private readonly ISettingView _view;
        public SettingPresenter(ISettingView view)
        {
            _view = view;
        }

        public Setting GetSettings(string code)
        {
            try
            {
                return new SettingRepository().GetSettings(code);
            }
            catch (Exception exp)
            {
                _view.ShowError(Defaults.AppProvider.AppTitle,exp.Message);
                LogWriter.LogWrite(exp);
                return new Setting();
            }
        }

        internal bool SaveSettings(List<Setting> lst)
        {
            try
            { 
                var teminalId = Guid.Parse(Defaults.TerminalId.ToString());
                return new SettingRepository().SaveSettings(lst, teminalId);
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
                return false;
            }
        }

        internal string BindCloudCleanCash()
        {
            try
            {
                string baseUrl = ConfigurationManager.AppSettings["Srv4Pos.RegisterCCU"];
                string userName = Defaults.APIUSER;
                string userPassword = Defaults.APIPassword;

                if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userPassword))
                    return UI.Cloud_Clean_Cash_Credentials;

                var teminalId = Guid.Parse(Defaults.TerminalId.ToString());
                Terminal dbTerminal = new TerminalRepository().GetTerminalById(teminalId);

                if (string.IsNullOrEmpty(dbTerminal.Description))
                    return UI.Cloud_Clean_Cash_Terminal_Description;

                if (!string.IsNullOrEmpty(dbTerminal.CCUData))
                    return UI.Cloud_Clean_Cash_Already_Registered;

                TerminalApi terminal = TerminalApi.ConvertModelToApiModel(dbTerminal);
                terminal.ApplicationNameAndVersion = "POSSUM SYSTEM SE 1.03";

                bool isLoginResponse = false;

                TokenApi objToken = null;

                List<KeyValuePair<string, string>> data = new List<KeyValuePair<string, string>>();

                data.Add(new KeyValuePair<string, string>("grant_type", "password"));
                data.Add(new KeyValuePair<string, string>("username", userName));
                data.Add(new KeyValuePair<string, string>("password", userPassword));
                data.Add(new KeyValuePair<string, string>("imei", ""));

                using (var httpClient = new HttpClient())
                {
                    using (var content = new FormUrlEncodedContent(data))
                    {
                        content.Headers.Clear();
                        content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                        HttpResponseMessage response = httpClient.PostAsync(baseUrl + "/Login", content).Result;

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var result = response.Content.ReadAsStringAsync().Result;
                            objToken = JsonConvert.DeserializeObject<TokenApi>(result);
                            isLoginResponse = true;
                        }
                    }
                }

                if (isLoginResponse)
                {
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", objToken.access_token);
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                        string json = JsonConvert.SerializeObject(terminal);
                        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
                        stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        HttpResponseMessage response = httpClient.PostAsync(baseUrl + "/Terminal/PostTerminal", stringContent).Result;

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var result = response.Content.ReadAsStringAsync().Result;
                            terminal = JsonConvert.DeserializeObject<TerminalApi>(result);
                            new TerminalRepository().UpdateTerminalCCUData(teminalId, terminal.CCUData);

                            return "Success";
                        }
                    }
                }

                return UI.Cloud_Clean_Cash_Already_Failed;
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
                return UI.Cloud_Clean_Cash_Already_Failed;
            }
        }
    }
}