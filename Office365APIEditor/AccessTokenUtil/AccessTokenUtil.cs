﻿// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information. 

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace Office365APIEditor.AccessTokenUtil
{
    public class AccessTokenUtil
    {
        protected AcquireAuthorizationCodeResult AcquireAuthorizationCode(GetCodeForm AuthorizationCodeRequester)
        {
            if (AuthorizationCodeRequester.ShowDialog(out string Code) == DialogResult.OK)
            {
                if (Code != "")
                {
                    return new AcquireAuthorizationCodeResult()
                    {
                        Success = InteractiveResult.Success,
                        AuthorizationCode = Code,
                        ErrorMessage = ""
                    };
                }
                else
                {
                    return new AcquireAuthorizationCodeResult()
                    {
                        Success = InteractiveResult.Fail,
                        AuthorizationCode = "",
                        ErrorMessage = "Getting Authorization Code was failed."
                    };
                }
            }
            else
            {
                return new AcquireAuthorizationCodeResult()
                {
                    Success = InteractiveResult.Cancel,
                    AuthorizationCode = "",
                    ErrorMessage = "The user canceled the authentication window."
                };
            }
        }

        protected AcquireAccessTokenResult AcquireAccessToken(string PostBody, string EndPointUrl)
        {
            InteractiveResult result = InteractiveResult.Fail;
            TokenResponse token = null;
            string errorMessage = "";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(EndPointUrl);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = Util.CustomUserAgent;

            try
            {
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(PostBody);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.Default);
                    string jsonResponse = reader.ReadToEnd();

                    // Deserialize and get an Access Token.
                    token = Util.Deserialize<TokenResponse>(jsonResponse);
                }
            }
            catch (WebException ex)
            {
                WebResponse response = ex.Response;
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.Default);
                    string jsonResponse = reader.ReadToEnd();

                    errorMessage = ex.Message + "\r\n\r\n" + ex.StackTrace + "\r\n\r\n" + jsonResponse;
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message + "\r\n\r\n" + ex.StackTrace;
            }

            if (errorMessage == "")
            {
                result = InteractiveResult.Success;
            }

            return new AcquireAccessTokenResult()
            {
                Success = result,
                Token = token,
                ErrorMessage = errorMessage
            };
        }

    }

    public struct ValidateResult
    {
        public bool IsValid;
        public string[] ErrorMessage;
    }

    public enum InteractiveResult
    {
        Fail,
        Success,
        Cancel
    }

    public struct AcquireAuthorizationCodeResult
    {
        public InteractiveResult Success;
        public string AuthorizationCode;
        public string ErrorMessage;
    }

    public struct AcquireAccessTokenResult
    {
        public InteractiveResult Success;
        public TokenResponse Token;
        public string ErrorMessage;
    }
}
