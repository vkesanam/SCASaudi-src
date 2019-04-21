using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Description;
using System.Web;

namespace LuisBot
{
    public class CRMConnection
    {
        internal static void CreateLeadReg(string customerName, string email,string phone, string membershipdetails)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

                ClientCredentials credentials = new ClientCredentials();
                credentials.UserName.UserName = "admin@scasacrm.onmicrosoft.com";
                credentials.UserName.Password = "Welcome@123";
                Uri OrganizationUri = new Uri("https://scasacrm.api.crm4.dynamics.com/XRMServices/2011/Organization.svc");
                Uri HomeRealUri = null;
                using (OrganizationServiceProxy serviceProxy = new OrganizationServiceProxy(OrganizationUri, HomeRealUri, credentials, null))
                {
                    IOrganizationService service = (IOrganizationService)serviceProxy;

                    //CrmServiceClient crmConn = new CrmServiceClient("admin@scasacrm.onmicrosoft.com", CrmServiceClient.MakeSecureString("Welcome@123"), "EMEA", "orgdc02b016", useUniqueInstance: false, useSsl: true, isOffice365: true);
                    //IOrganizationService service = crmConn.OrganizationServiceProxy;

                    Entity Lead = new Entity("lead");
                    Lead["subject"] = "Request for New Membership - " + membershipdetails;
                    Lead["lastname"] = customerName;
                    Lead["emailaddress1"] = email;
                    Lead["mobilephone"] = phone;
                    Guid LeadGuid = service.Create(Lead);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static void CreateCase(string complaint, string customerName, string phone, string email)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
                ClientCredentials credentials = new ClientCredentials();
                credentials.UserName.UserName = "admin@scasacrm.onmicrosoft.com";
                credentials.UserName.Password = "Welcome@123";
                Uri OrganizationUri = new Uri("https://scasacrm.api.crm4.dynamics.com/XRMServices/2011/Organization.svc");
                Uri HomeRealUri = null;
                using (OrganizationServiceProxy serviceProxy = new OrganizationServiceProxy(OrganizationUri, HomeRealUri, credentials, null))
                {
                    IOrganizationService service = (IOrganizationService)serviceProxy;
                    //CrmServiceClient crmConn = new CrmServiceClient("admin@scasacrm.onmicrosoft.com", CrmServiceClient.MakeSecureString("Welcome@123"), "EMEA", "orgdc02b016", useUniqueInstance: false, useSsl: true, isOffice365: true);
                    //IOrganizationService service = crmConn.OrganizationServiceProxy;

                    Microsoft.Xrm.Sdk.Entity Case = new Microsoft.Xrm.Sdk.Entity("incident");
                    Case["title"] = complaint;

                    Microsoft.Xrm.Sdk.Entity Account = new Microsoft.Xrm.Sdk.Entity("account");
                    Account["name"] = customerName;
                    Account["telephone1"] = phone;
                    Account["emailaddress1"] = email;
                    Guid AccountId = service.Create(Account);

                    Case["customerid"] = new EntityReference("account", AccountId);
                    Guid CaseId = service.Create(Case);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}