// Settings for app to be loaded into the global scope
// QA Configuration
// We could make this server agnostic by declaring these server-side but this is a js spa so we will simply switch the config file out at deploy time

var prefix = 'pd'; // Dummy CI Deployment prefix 
var clientid = ''; // Dummy CI Deployment Azure AD App 
var tenantid = 'microsoft.com'; // Azure AD Tenant

var instance = 'https://login.microsoftonline.com/'; // Azure AD logon endpoint
var dateform = "DD/MM/YYYY"; // UK locale
var timeform = "HH:mm"; // 12h UK time

// Pick up server specific settings declared in config.js and make them usable in this script
var resource = 'https://' + prefix + 'ukohfn.azurewebsites.net';
var endpoint = 'https://' + prefix + '-ukofficehours.azurewebsites.net/';
var rootfnsite = resource + "/";