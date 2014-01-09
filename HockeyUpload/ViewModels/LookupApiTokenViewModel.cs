﻿using Caliburn.Micro;
using HockeyApp.AppLoader.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using HockeyApp.AppLoader.Extensions;

namespace HockeyApp.AppLoader.ViewModels
{
    public class LookupApiTokenViewModel:ViewModelBase
    {
        private string _apiBase = "";
        public LookupApiTokenViewModel(string apiBase)
        {
            this._apiBase = apiBase;
            base.DisplayName = "Lookup api token";
        }

        private string _username = "";
        public string Username
        {
            get
            {
                return this._username;
            }
            set
            {
                this._username = value;
                NotifyOfPropertyChange(() => this.Username);
                NotifyOfPropertyChange(() => this.CanLookupTokens);
            }
        }

        private string _password = "";
        public string Password
        {
            get
            {
                return this._password;
            }
            set
            {
                this._password = value;
                NotifyOfPropertyChange(() => this.Password);
                NotifyOfPropertyChange(() => this.CanLookupTokens);
            }
        }

        public bool IsInErrorState   {
            get { return !String.IsNullOrWhiteSpace(this.ErrorMessage); }
        }

        private string _errorMessage="";
        public string ErrorMessage
        {
            get { return this._errorMessage; }
            set
            {
                this._errorMessage = value;
                NotifyOfPropertyChange(() => this.ErrorMessage);
                NotifyOfPropertyChange(() => this.IsInErrorState);
            }
        }

        public List<ApiToken> ApiTokens { get; private set; }
        private ApiToken _selectedApiToken = null;
        public ApiToken SelectedApiToken
        {
            get { return this._selectedApiToken; }
            set
            {
                this._selectedApiToken = value;
                NotifyOfPropertyChange(() => this.SelectedApiToken);
                NotifyOfPropertyChange(() => this.CanOK);
            }
        }

        private bool _viewIsBusy = false;
        public bool ViewIsBusy
        {
            get { return _viewIsBusy; }
            set
            {
                _viewIsBusy = value;
                NotifyOfPropertyChange(() => ViewIsBusy);
            }
        }

        private string _busyMessage = "";
        public string BusyMessage
        {
            get { return this._busyMessage; }
            set
            {
                this._busyMessage = value;
                NotifyOfPropertyChange(() => BusyMessage);
            }
        }       


        #region Commands

        public async Task LookupTokens()
        {
            this.ApiTokens = null;
            this.SelectedApiToken = null;
            this.ErrorMessage = "";
            this.BusyMessage = "Loading...";
            this.ViewIsBusy = true;
            try
            {
                ApiTokenEnvelope envelope = await ApiTokenEnvelope.Load(this._apiBase, this.Username, this.Password);
                this.ApiTokens = envelope.Tokens.Where(p => p.Rights.Equals("0") || p.Rights.Equals("1")).ToList();
                this.ApiTokens.Insert(0, new CreateNewApiToken());
                NotifyOfPropertyChange(() => this.ApiTokens);
            }
            catch(Exception ex)
            {
                this.ErrorMessage = ex.Message;
            }
            finally
            {
                this.ViewIsBusy = false;
                this.BusyMessage = "";
            }
        }

        public bool CanLookupTokens { get { return !String.IsNullOrWhiteSpace(this.Username) && !String.IsNullOrWhiteSpace(this.Password); } }

        public async void OK()
        {
            if (this.SelectedApiToken is CreateNewApiToken) {
                this.BusyMessage = "Loading...";
                this.ViewIsBusy = true;
                try
                {
                    ApiToken newToken = await ApiTokenEnvelope.CreateNewToken(this._apiBase, this.Username, this.Password);
                    await this.LookupTokens();
                    ApiToken toSelect = this.ApiTokens.FirstOrDefault(p => p.Token.Equals(newToken.Token));
                    if (toSelect != null)
                    {
                        this.SelectedApiToken = toSelect;
                    }
                    else
                    {
                        this.SelectedApiToken = this.ApiTokens[0];
                    }
                }
                catch (Exception ex)
                {
                    this.ErrorMessage = ex.Message;
                }
                finally
                {
                    this.ViewIsBusy = false;
                    this.BusyMessage = "";
                }
            }
            else
            {
                this.TryClose(true);
            }
        }

        public bool CanOK { get { return this.SelectedApiToken != null; } }

        public void Cancel()
        {
            this.TryClose(false);
        }
        #endregion

    }

    public class CreateNewApiToken : ApiToken
    {
        public override string Token
        {
            get
            {
                return "";
            }
        }
        public override string NameToDisplay { get { return "create new API token..."; } }
        public override string RightsDisplay
        {
            get
            {
                return "";
            }
        }
    }
}
